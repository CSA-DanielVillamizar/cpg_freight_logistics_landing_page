using CPG.Domain.Common;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CPG.Domain.UnitTests;

public sealed class DomainPrimitivesTests
{
    [Fact]
    public void New_entity_gets_a_non_empty_identity()
    {
        var lead = Lead.RegisterFromLandingPage(
            "Apex Construction",
            "Alex Apex",
            "contact@apex.com",
            "(407) 555-0100",
            "fdot-concrete-barricades",
            ServiceType.FdotConcrete,
            "120 concrete Jersey barricades, Orlando to Tampa",
            DateTimeOffset.UtcNow);

        lead.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Landing_page_lead_starts_New_and_raises_a_domain_event()
    {
        var lead = Lead.RegisterFromLandingPage(
            "Apex Construction",
            "Alex Apex",
            "contact@apex.com",
            "(407) 555-0100",
            "fdot-concrete-barricades",
            ServiceType.FdotConcrete,
            "120 concrete Jersey barricades, Orlando to Tampa",
            DateTimeOffset.UtcNow);

        lead.Status.Should().Be(LeadStatus.New);
        lead.ContactEmail.Should().Be("contact@apex.com");
        lead.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Transactional_entities_expose_an_xmin_row_version()
    {
        typeof(Carrier).Should().BeAssignableTo<IHasRowVersion>();
        typeof(Load).Should().BeAssignableTo<IHasRowVersion>();
    }

    [Fact]
    public void Aggregate_root_records_and_clears_domain_events()
    {
        var carrier = new Carrier { CompanyName = "Southern Civil", UserId = Guid.NewGuid() };

        carrier.DomainEvents.Should().BeEmpty();
        carrier.ClearDomainEvents();
        carrier.DomainEvents.Should().BeEmpty();
    }
}
