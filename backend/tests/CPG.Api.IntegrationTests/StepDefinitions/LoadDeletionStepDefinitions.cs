using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CPG.Api.IntegrationTests.Support;
using CPG.Application.Features.Authentication;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using CPG.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;

namespace CPG.Api.IntegrationTests.StepDefinitions;

[Binding]
public sealed class LoadDeletionStepDefinitions(ScenarioState state)
{
    private readonly Dictionary<string, Guid> _loadIds = new(StringComparer.Ordinal);

    // ---- Given ----

    [Given(@"a delivered load ""(.*)"" billed to the shipper with a pending invoice")]
    public async Task GivenADeliveredLoadWithInvoice(string reference)
    {
        var shipperUserId = await TestScope.WithDbContextAsync(db => db.Users
            .Where(u => u.Email == "shipper@cpgorlando.com")
            .Select(u => u.Id)
            .FirstAsync());

        var carrierId = await TestScope.WithDbContextAsync(db => db.Carriers
            .Select(c => c.Id)
            .FirstAsync());

        var loadId = await TestScope.WithDbContextAsync(async db =>
        {
            var existing = await db.Loads.IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Reference == reference);
            if (existing is not null)
            {
                var staleInvoices = await db.Invoices.IgnoreQueryFilters()
                    .Where(i => i.LoadId == existing.Id).ToListAsync();
                db.Invoices.RemoveRange(staleInvoices);
                db.Loads.Remove(existing);
                await db.SaveChangesAsync();
            }

            var load = new Load
            {
                Reference = reference,
                ServiceType = ServiceType.HeavyHaul,
                EquipmentType = "RGN Multi-Axle",
                OriginCity = "Orlando",
                OriginState = "FL",
                OriginZip = "32801",
                DestinationCity = "Atlanta",
                DestinationState = "GA",
                DestinationZip = "30301",
                DistanceMiles = 438,
                WeightLbs = 51000,
                RateUsd = 3900m,
                ShipperName = "BDD Shipper Co.",
                ShipperUserId = shipperUserId,
                AssignedCarrierId = carrierId,
                PickupAtUtc = DateTimeOffset.UtcNow.AddDays(-3),
                DeliveryAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
                Status = LoadStatus.Delivered,
            };
            db.Loads.Add(load);

            var invoice = Invoice.ForDeliveredLoad(load, $"INV-{reference[4..]}", DateTimeOffset.UtcNow);
            db.Invoices.Add(invoice);

            await db.SaveChangesAsync();
            return load.Id;
        });

        _loadIds[reference] = loadId;
    }

    [Given(@"a fresh available load ""(.*)"" from ""(.*)"" to ""(.*)""")]
    public async Task GivenAFreshAvailableLoad(string reference, string origin, string destination)
    {
        var (originCity, originState) = SplitStop(origin);
        var (destinationCity, destinationState) = SplitStop(destination);

        var loadId = await TestScope.WithDbContextAsync(async db =>
        {
            var existing = await db.Loads.IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Reference == reference);
            if (existing is not null)
            {
                db.Loads.Remove(existing);
                await db.SaveChangesAsync();
            }

            var load = new Load
            {
                Reference = reference,
                ServiceType = ServiceType.StandardDryVan,
                EquipmentType = "53' Dry Van",
                OriginCity = originCity,
                OriginState = originState,
                OriginZip = "33602",
                DestinationCity = destinationCity,
                DestinationState = destinationState,
                DestinationZip = "31201",
                DistanceMiles = 400,
                WeightLbs = 25000,
                RateUsd = 1500m,
                ShipperName = "BDD Shipper Co.",
                PickupAtUtc = DateTimeOffset.UtcNow.AddDays(2),
                DeliveryAtUtc = DateTimeOffset.UtcNow.AddDays(3),
                Status = LoadStatus.Available,
            };
            db.Loads.Add(load);
            await db.SaveChangesAsync();
            return load.Id;
        });

        _loadIds[reference] = loadId;
    }

    [Given(@"a synthetic load ""(.*)"" exists in the database")]
    public async Task GivenASyntheticLoad(string reference)
    {
        var loadId = await TestScope.WithDbContextAsync(async db =>
        {
            var existing = await db.Loads.IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Reference == reference);
            if (existing is not null)
            {
                return existing.Id;
            }

            var load = new Load
            {
                Reference = reference,
                ServiceType = ServiceType.Flatbed,
                EquipmentType = "48' Flatbed",
                OriginCity = "Miami",
                OriginState = "FL",
                OriginZip = "33101",
                DestinationCity = "Dallas",
                DestinationState = "TX",
                DestinationZip = "75201",
                DistanceMiles = 1300,
                WeightLbs = 30000,
                RateUsd = 3200m,
                ShipperName = "Synthetic Fixture",
                PickupAtUtc = DateTimeOffset.UtcNow.AddDays(1),
                DeliveryAtUtc = DateTimeOffset.UtcNow.AddDays(3),
                Status = LoadStatus.Available,
            };
            db.Loads.Add(load);
            await db.SaveChangesAsync();
            return load.Id;
        });

        _loadIds[reference] = loadId;
    }

    // ---- When ----

    [When(@"an admin deletes load ""(.*)""")]
    public async Task WhenAnAdminDeletesLoad(string reference)
    {
        await AuthenticateAsync("admin@cpgorlando.com");
        state.LastResponse = await state.Client.DeleteAsync($"/api/loads/{_loadIds[reference]}");
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();
    }

    [When(@"the carrier attempts to delete load ""(.*)""")]
    public async Task WhenTheCarrierAttemptsToDelete(string reference)
    {
        await AuthenticateAsync("carrier@cpgorlando.com");
        state.LastResponse = await state.Client.DeleteAsync($"/api/loads/{_loadIds[reference]}");
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();
    }

    [When(@"the admin requests the load board")]
    public async Task WhenTheAdminRequestsTheBoard()
    {
        await AuthenticateAsync("admin@cpgorlando.com");
        state.LastResponse = await state.Client.GetAsync("/api/loads");
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();
        state.LastResponse.StatusCode.Should().Be(HttpStatusCode.OK, state.LastBody);
    }

    [When(@"the admin requests the admin load list")]
    public async Task WhenTheAdminRequestsTheAdminLoadList()
    {
        await AuthenticateAsync("admin@cpgorlando.com");
        state.LastResponse = await state.Client.GetAsync("/api/admin/loads");
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();
        state.LastResponse.StatusCode.Should().Be(HttpStatusCode.OK, state.LastBody);
    }

    // ---- Then ----

    [Then(@"the delete request succeeds with status (\d+)")]
    public void ThenTheDeleteSucceeds(int statusCode)
        => ((int)state.LastResponse!.StatusCode).Should().Be(statusCode, state.LastBody);

    [Then(@"the delete is rejected with status (\d+)")]
    public void ThenTheDeleteIsRejected(int statusCode)
        => ((int)state.LastResponse!.StatusCode).Should().Be(statusCode, state.LastBody);

    [Then(@"load ""(.*)"" is not on the load board")]
    public async Task ThenLoadIsNotOnTheBoard(string reference)
    {
        await AuthenticateAsync("admin@cpgorlando.com");
        var response = await state.Client.GetAsync("/api/loads");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.EnumerateArray()
            .Select(r => r.GetProperty("reference").GetString())
            .Should().NotContain(reference);
    }

    [Then(@"load ""(.*)"" is not in the shipper portal")]
    public async Task ThenLoadIsNotInShipperPortal(string reference)
    {
        await AuthenticateAsync("shipper@cpgorlando.com");

        var loads = await state.Client.GetAsync("/api/shipper/loads");
        using var loadDoc = JsonDocument.Parse(await loads.Content.ReadAsStringAsync());
        var allRefs = loadDoc.RootElement.GetProperty("active").EnumerateArray()
            .Concat(loadDoc.RootElement.GetProperty("history").EnumerateArray())
            .Select(r => r.GetProperty("reference").GetString())
            .ToList();
        allRefs.Should().NotContain(reference);

        var invoices = await state.Client.GetAsync("/api/shipper/invoices");
        using var invoiceDoc = JsonDocument.Parse(await invoices.Content.ReadAsStringAsync());
        invoiceDoc.RootElement.GetProperty("invoices").EnumerateArray()
            .Select(i => i.GetProperty("loadReference").GetString())
            .Should().NotContain(reference);
    }

    [Then(@"the invoice for ""(.*)"" is soft-deleted and Cancelled")]
    public async Task ThenTheInvoiceIsCancelled(string reference)
    {
        var invoice = await TestScope.WithDbContextAsync(db => db.Invoices
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstAsync(i => i.LoadId == _loadIds[reference]));

        invoice.IsDeleted.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Then(@"an audit log entry ""(.*)"" exists for load ""(.*)""")]
    public async Task ThenAnAuditEntryExists(string action, string reference)
    {
        var loadId = _loadIds[reference].ToString();
        var entry = await TestScope.WithDbContextAsync(db => db.AuditLogEntries
            .AsNoTracking()
            .Where(a => a.Action == action && a.EntityName == nameof(Load) && a.EntityId == loadId)
            .OrderByDescending(a => a.TimestampUtc)
            .FirstOrDefaultAsync());

        entry.Should().NotBeNull();
    }

    [Then(@"load ""(.*)"" is in the admin load list and flagged synthetic")]
    public void ThenLoadIsInAdminListFlaggedSynthetic(string reference)
    {
        using var document = JsonDocument.Parse(state.LastBody!);
        var row = document.RootElement.EnumerateArray()
            .FirstOrDefault(r => r.GetProperty("reference").GetString() == reference);

        row.ValueKind.Should().NotBe(JsonValueKind.Undefined, $"{reference} should be in the admin list");
        row.GetProperty("isSynthetic").GetBoolean().Should().BeTrue();
    }

    // ---- helpers ----

    private async Task AuthenticateAsync(string email)
    {
        var login = await state.Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = ApplicationDbContextInitialiser.SeedPassword,
        });
        login.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        state.Authenticate(document.RootElement.GetProperty("accessToken").GetString()!);
    }

    private static (string City, string State) SplitStop(string value)
    {
        var parts = value.Split(',', 2, StringSplitOptions.TrimEntries);
        return (parts[0], parts[1]);
    }
}
