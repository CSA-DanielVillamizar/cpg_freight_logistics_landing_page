using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CPG.Api.IntegrationTests.Support;
using CPG.Application.Features.Leads;
using CPG.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;

namespace CPG.Api.IntegrationTests.StepDefinitions;

[Binding]
public sealed class LeadGenerationStepDefinitions(ScenarioState state)
{
    private static readonly IReadOnlyDictionary<string, string> VerticalNameToSlug =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FDOT Concrete Barricades"] = "fdot-concrete-barricades",
            ["Refrigerated & Cold Chain"] = "refrigerated-cold-chain",
            ["Heavy Haul & Flatbed"] = "flatbed-heavy-haul",
            ["Mobile Freight"] = "mobile-rate-calculator",
        };

    private string _verticalSlug = string.Empty;
    private string _companyName = string.Empty;
    private string _contactEmail = string.Empty;
    private string _cargoDetails = string.Empty;
    private Guid _leadId;

    [Given(@"a prospective client visits the ""(.*)"" vertical landing page")]
    public void GivenAProspectiveClientVisitsTheVerticalLandingPage(string verticalName)
    {
        _verticalSlug = VerticalNameToSlug[verticalName];
    }

    [When(@"the client fills out the contact form with company name ""(.*)"", email ""(.*)"", and cargo details")]
    public void WhenTheClientFillsOutTheContactForm(string companyName, string email)
    {
        _companyName = companyName;
        _contactEmail = email;
        _cargoDetails = "480 linear feet of FDOT Index 102-100 K-rail, night shift, Orange County I-4 corridor";
    }

    [When(@"submits the form via POST ""(.*)""")]
    public async Task WhenSubmitsTheFormViaPost(string path)
    {
        var payload = new CreateLeadRequest
        {
            CompanyName = _companyName,
            ContactName = "Alex Apex",
            ContactEmail = _contactEmail,
            Phone = "(407) 555-0100",
            VerticalSlug = _verticalSlug,
            ServiceType = ServiceType.FdotConcrete,
            CargoDetails = _cargoDetails,
        };

        state.LastResponse = await state.Client.PostAsJsonAsync(path, payload);
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();
    }

    [Then(@"the system should validate all mandatory fields successfully")]
    public void ThenTheSystemShouldValidateAllMandatoryFieldsSuccessfully()
    {
        state.LastResponse.Should().NotBeNull();
        state.LastResponse!.StatusCode.Should().Be(HttpStatusCode.OK, state.LastBody);

        using var document = JsonDocument.Parse(state.LastBody!);
        _leadId = document.RootElement.GetProperty("id").GetGuid();
        document.RootElement.GetProperty("status").GetString().Should().Be("New");
    }

    [Then(@"save the lead record in the PostgreSQL database with status ""(.*)""")]
    public async Task ThenSaveTheLeadRecordWithStatus(string status)
    {
        var expected = Enum.Parse<LeadStatus>(status);

        var lead = await TestScope.WithDbContextAsync(db => db.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == _leadId));

        lead.Should().NotBeNull();
        lead!.Status.Should().Be(expected);
        lead.CompanyName.Should().Be(_companyName);
        lead.ContactEmail.Should().Be(_contactEmail.ToLowerInvariant());
        lead.VerticalSlug.Should().Be(_verticalSlug);
    }

    [Then(@"dispatch an asynchronous event via RabbitMQ to notify the commercial team")]
    public async Task ThenDispatchAnAsynchronousEventViaRabbitMq()
    {
        var notified = await TestScope.EventuallyAsync(
            () => TestScope.WithDbContextAsync(db => db.AuditLogEntries
                .AsNoTracking()
                .AnyAsync(a => a.Action == "CommercialTeamNotified"
                    && a.EntityName == "Lead"
                    && a.EntityId == _leadId.ToString())),
            TimeSpan.FromSeconds(20));

        notified.Should().BeTrue("the CorporateLeadGenerated integration event must be consumed from RabbitMQ");
    }
}
