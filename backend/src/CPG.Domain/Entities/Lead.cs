using CPG.Domain.Common;
using CPG.Domain.Enums;
using CPG.Domain.Events;

namespace CPG.Domain.Entities;

/// <summary>An enterprise inquiry captured by a niche vertical landing page (SPEC.md US-04).</summary>
public class Lead : AggregateRoot, IAuditableEntity
{
    public required string CompanyName { get; set; }

    public required string ContactName { get; set; }

    public required string ContactEmail { get; set; }

    public required string Phone { get; set; }

    /// <summary>Slug of the originating vertical page, e.g. <c>fdot-concrete-barricades</c>.</summary>
    public required string VerticalSlug { get; set; }

    public ServiceType? ServiceType { get; set; }

    public string? CargoDetails { get; set; }

    public LeadStatus Status { get; private set; } = LeadStatus.New;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Factory for a lead arriving from a public landing page. Starts in <see cref="LeadStatus.New"/>
    /// and raises <see cref="CorporateLeadGeneratedDomainEvent"/> (SPEC.md US-04).
    /// </summary>
    public static Lead RegisterFromLandingPage(
        string companyName,
        string contactName,
        string contactEmail,
        string phone,
        string verticalSlug,
        ServiceType? serviceType,
        string? cargoDetails,
        DateTimeOffset createdAtUtc)
    {
        var lead = new Lead
        {
            CompanyName = companyName.Trim(),
            ContactName = contactName.Trim(),
            ContactEmail = contactEmail.Trim().ToLowerInvariant(),
            Phone = phone.Trim(),
            VerticalSlug = verticalSlug.Trim().ToLowerInvariant(),
            ServiceType = serviceType,
            CargoDetails = cargoDetails?.Trim(),
            Status = LeadStatus.New,
            CreatedAtUtc = createdAtUtc,
        };

        lead.RaiseDomainEvent(new CorporateLeadGeneratedDomainEvent(
            lead.Id,
            lead.CompanyName,
            lead.ContactEmail,
            lead.VerticalSlug,
            lead.ServiceType));

        return lead;
    }
}
