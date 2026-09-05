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
public sealed class LoadBoardStepDefinitions(ScenarioState state)
{
    private const string CarrierEmail = "carrier@cpgorlando.com";

    private Guid _carrierUserId;
    private Guid _carrierId;
    private string _lastAcceptedReference = string.Empty;
    private readonly Dictionary<string, Guid> _loadIds = new(StringComparer.Ordinal);

    [Given(@"an authenticated Carrier on the load board")]
    public async Task GivenAnAuthenticatedCarrier()
    {
        var login = await state.Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = CarrierEmail,
            Password = ApplicationDbContextInitialiser.SeedPassword,
        });
        login.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        state.Authenticate(document.RootElement.GetProperty("accessToken").GetString()!);
        _carrierUserId = document.RootElement.GetProperty("user").GetProperty("id").GetGuid();

        _carrierId = await TestScope.WithDbContextAsync(db => db.Carriers
            .Where(c => c.UserId == _carrierUserId)
            .Select(c => c.Id)
            .FirstAsync());
    }

    [Given(@"an available load ""(.*)"" from ""(.*)"" to ""(.*)""")]
    public async Task GivenAnAvailableLoad(string reference, string origin, string destination)
    {
        var (originCity, originState) = SplitStop(origin);
        var (destinationCity, destinationState) = SplitStop(destination);

        await UpsertLoadAsync(new Load
        {
            Reference = reference,
            ServiceType = ServiceType.StandardDryVan,
            EquipmentType = "53' Dry Van",
            OriginCity = originCity,
            OriginState = originState,
            OriginZip = "33101",
            DestinationCity = destinationCity,
            DestinationState = destinationState,
            DestinationZip = "75201",
            DistanceMiles = 1300,
            WeightLbs = 28000,
            RateUsd = 3200m,
            ShipperName = "BDD Freight Co.",
            PickupAtUtc = DateTimeOffset.UtcNow.AddDays(2),
            DeliveryAtUtc = DateTimeOffset.UtcNow.AddDays(4),
            Status = LoadStatus.Available,
        });
    }

    [Given(@"an already delivered load ""(.*)""")]
    public async Task GivenADeliveredLoad(string reference)
    {
        await UpsertLoadAsync(new Load
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
            WeightLbs = 52000,
            RateUsd = 4100m,
            ShipperName = "BDD Freight Co.",
            PickupAtUtc = DateTimeOffset.UtcNow.AddDays(-4),
            DeliveryAtUtc = DateTimeOffset.UtcNow.AddDays(-2),
            Status = LoadStatus.Delivered,
        });
    }

    [When(@"the carrier requests the board filtered by status ""(.*)""")]
    public async Task WhenTheCarrierRequestsTheBoard(string status)
    {
        state.LastResponse = await state.Client.GetAsync($"/api/loads?status={status}");
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();
        state.LastResponse.StatusCode.Should().Be(HttpStatusCode.OK, state.LastBody);
    }

    [Then(@"the board response includes load ""(.*)""")]
    public void ThenTheBoardIncludesLoad(string reference)
    {
        using var document = JsonDocument.Parse(state.LastBody!);
        var references = document.RootElement.EnumerateArray()
            .Select(row => row.GetProperty("reference").GetString())
            .ToList();

        references.Should().Contain(reference);
    }

    [When(@"the carrier accepts load ""(.*)""")]
    public async Task WhenTheCarrierAcceptsLoad(string reference)
    {
        _lastAcceptedReference = reference;
        var id = _loadIds[reference];
        state.LastResponse = await state.Client.PostAsync($"/api/loads/{id}/accept", content: null);
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();
    }

    [Then(@"the accept response reports status ""(.*)""")]
    public void ThenTheAcceptResponseReportsStatus(string status)
    {
        state.LastResponse!.StatusCode.Should().Be(HttpStatusCode.OK, state.LastBody);
        using var document = JsonDocument.Parse(state.LastBody!);
        document.RootElement.GetProperty("status").GetString().Should().Be(status);
    }

    [Then(@"the load ""(.*)"" is assigned to the carrier in PostgreSQL")]
    public async Task ThenTheLoadIsAssigned(string reference)
    {
        var id = _loadIds[reference];
        var load = await TestScope.WithDbContextAsync(db => db.Loads
            .AsNoTracking()
            .FirstAsync(l => l.Id == id));

        load.Status.Should().Be(LoadStatus.Dispatched);
        load.AssignedCarrierId.Should().Be(_carrierId);
    }

    [Then(@"an audit log entry ""(.*)"" is recorded for the load")]
    public async Task ThenAnAuditLogEntryIsRecorded(string action)
    {
        var loadId = _loadIds[_lastAcceptedReference].ToString();

        var entry = await TestScope.WithDbContextAsync(db => db.AuditLogEntries
            .AsNoTracking()
            .Where(a => a.Action == action && a.EntityName == nameof(Load) && a.EntityId == loadId)
            .OrderByDescending(a => a.TimestampUtc)
            .FirstOrDefaultAsync());

        entry.Should().NotBeNull();
        entry!.UserId.Should().Be(_carrierUserId.ToString());
        entry.TimestampUtc.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-5));
    }

    [Then(@"the dispatch desk is notified through the broker")]
    public async Task ThenTheDispatchDeskIsNotified()
    {
        var loadId = _loadIds[_lastAcceptedReference].ToString();

        var notified = await TestScope.EventuallyAsync(
            () => TestScope.WithDbContextAsync(db => db.AuditLogEntries
                .AsNoTracking()
                .AnyAsync(a => a.Action == "DispatchDeskNotified" && a.EntityId == loadId)),
            TimeSpan.FromSeconds(20));

        notified.Should().BeTrue("the LoadAccepted integration event must be consumed from the broker");
    }

    [Then(@"the request fails with status (\d+)")]
    public void ThenTheRequestFailsWithStatus(int statusCode)
        => ((int)state.LastResponse!.StatusCode).Should().Be(statusCode, state.LastBody);

    private static (string City, string State) SplitStop(string value)
    {
        var parts = value.Split(',', 2, StringSplitOptions.TrimEntries);
        return (parts[0], parts[1]);
    }

    private async Task UpsertLoadAsync(Load load)
    {
        await TestScope.WithDbContextAsync(async db =>
        {
            var existing = await db.Loads.FirstOrDefaultAsync(l => l.Reference == load.Reference);
            if (existing is not null)
            {
                db.Loads.Remove(existing);
                await db.SaveChangesAsync();
            }

            db.Loads.Add(load);
            await db.SaveChangesAsync();
        });

        _loadIds[load.Reference] = load.Id;
    }
}
