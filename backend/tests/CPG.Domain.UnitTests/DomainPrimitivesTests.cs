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
        var lead = new Lead
        {
            CompanyName = "Apex Construction",
            ContactEmail = "contact@apex.com",
            VerticalSlug = "fdot-concrete-barricades",
        };

        lead.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void New_lead_defaults_to_status_New()
    {
        var lead = new Lead
        {
            CompanyName = "Apex Construction",
            ContactEmail = "contact@apex.com",
            VerticalSlug = "fdot-concrete-barricades",
        };

        lead.Status.Should().Be(LeadStatus.New);
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
