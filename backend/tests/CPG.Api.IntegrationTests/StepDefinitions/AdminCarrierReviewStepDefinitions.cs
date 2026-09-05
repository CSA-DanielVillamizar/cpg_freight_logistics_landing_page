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
public sealed class AdminCarrierReviewStepDefinitions(ScenarioState state)
{
    private const string AdminEmail = "admin@cpgorlando.com";

    private Guid _adminUserId;
    private readonly Dictionary<string, Guid> _carrierIds = new(StringComparer.Ordinal);
    private string _lastReviewedCompany = string.Empty;

    [Given(@"an administrator is authenticated")]
    public async Task GivenAnAdministratorIsAuthenticated()
    {
        var login = await state.Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = AdminEmail,
            Password = ApplicationDbContextInitialiser.SeedPassword,
        });
        login.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        state.Authenticate(document.RootElement.GetProperty("accessToken").GetString()!);
        _adminUserId = document.RootElement.GetProperty("user").GetProperty("id").GetGuid();
    }

    [Given(@"a carrier ""(.*)"" with a filed COI is under review")]
    public async Task GivenACarrierUnderReview(string companyName)
    {
        await CreateCarrierAsync(companyName, withDocument: true);
    }

    [Given(@"a carrier ""(.*)"" with no documents")]
    public async Task GivenACarrierWithNoDocuments(string companyName)
    {
        await CreateCarrierAsync(companyName, withDocument: false);
    }

    [When(@"the administrator lists carriers filtered by status ""(.*)""")]
    public async Task WhenTheAdministratorListsCarriers(string status)
    {
        state.LastResponse = await state.Client.GetAsync($"/api/admin/carriers?status={status}");
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();
        state.LastResponse.StatusCode.Should().Be(HttpStatusCode.OK, state.LastBody);
    }

    [Then(@"the carrier ""(.*)"" appears in the list")]
    public void ThenTheCarrierAppearsInTheList(string companyName)
    {
        using var document = JsonDocument.Parse(state.LastBody!);
        var names = document.RootElement.EnumerateArray()
            .Select(row => row.GetProperty("companyName").GetString())
            .ToList();

        names.Should().Contain(companyName);
    }

    [When(@"the administrator approves the carrier")]
    public async Task WhenTheAdministratorApprovesTheCarrier()
    {
        var companyName = _carrierIds.Keys.Last();
        _lastReviewedCompany = companyName;
        var carrierId = _carrierIds[companyName];

        state.LastResponse = await state.Client.PostAsJsonAsync(
            $"/api/admin/carriers/{carrierId}/review",
            new { decision = "Approve", notes = "Docs verified against FMCSA." });
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();
    }

    [Then(@"the review response reports status ""(.*)""")]
    public void ThenTheReviewResponseReportsStatus(string status)
    {
        state.LastResponse!.StatusCode.Should().Be(HttpStatusCode.OK, state.LastBody);
        using var document = JsonDocument.Parse(state.LastBody!);
        document.RootElement.GetProperty("status").GetString().Should().Be(status);
    }

    [Then(@"the carrier's compliance status in PostgreSQL is ""(.*)""")]
    public async Task ThenTheCarrierStatusInPostgresIs(string status)
    {
        var expected = Enum.Parse<ComplianceStatus>(status);
        var carrierId = _carrierIds[_lastReviewedCompany];

        var actual = await TestScope.WithDbContextAsync(db => db.Carriers
            .AsNoTracking()
            .Where(c => c.Id == carrierId)
            .Select(c => c.ComplianceStatus)
            .FirstAsync());

        actual.Should().Be(expected);
    }

    [Then(@"an audit log entry ""(.*)"" is recorded for the carrier")]
    public async Task ThenAnAuditLogEntryIsRecorded(string action)
    {
        var carrierId = _carrierIds[_lastReviewedCompany].ToString();

        var entry = await TestScope.WithDbContextAsync(db => db.AuditLogEntries
            .AsNoTracking()
            .Where(a => a.Action == action && a.EntityName == nameof(Carrier) && a.EntityId == carrierId)
            .OrderByDescending(a => a.TimestampUtc)
            .FirstOrDefaultAsync());

        entry.Should().NotBeNull();
        entry!.UserId.Should().Be(_adminUserId.ToString());
        entry.TimestampUtc.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-5));
    }

    [Then(@"the review request fails with status (\d+)")]
    public void ThenTheReviewRequestFailsWithStatus(int statusCode)
        => ((int)state.LastResponse!.StatusCode).Should().Be(statusCode, state.LastBody);

    private async Task CreateCarrierAsync(string companyName, bool withDocument)
    {
        var carrierId = await TestScope.WithDbContextAsync(async db =>
        {
            var carrier = new Carrier
            {
                CompanyName = companyName,
                UserId = Guid.NewGuid(),
                DotNumber = "FL-ORL-TEST",
                McNumber = "MC-TEST",
            };
            db.Carriers.Add(carrier);

            if (withDocument)
            {
                var doc = carrier.SubmitComplianceDocument(
                    ComplianceDocumentType.CertificateOfInsurance,
                    "http://127.0.0.1:10000/devstoreaccount1/compliance-documents/test/coi.pdf",
                    "coi_insurance.pdf",
                    "application/pdf",
                    2_400_000,
                    DateTimeOffset.UtcNow);
                db.ComplianceDocuments.Add(doc);
            }

            await db.SaveChangesAsync();
            return carrier.Id;
        });

        _carrierIds[companyName] = carrierId;
    }
}
