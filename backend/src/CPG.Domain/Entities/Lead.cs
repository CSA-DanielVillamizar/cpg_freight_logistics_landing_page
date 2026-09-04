using CPG.Domain.Common;
using CPG.Domain.Enums;

namespace CPG.Domain.Entities;

/// <summary>An enterprise inquiry captured by a niche vertical landing page (SPEC.md US-04).</summary>
public class Lead : AggregateRoot, IAuditableEntity
{
    public required string CompanyName { get; set; }

    public required string ContactEmail { get; set; }

    public string? ContactName { get; set; }

    public string? Phone { get; set; }

    /// <summary>Slug of the originating vertical page, e.g. <c>fdot-concrete-barricades</c>.</summary>
    public required string VerticalSlug { get; set; }

    public ServiceType? ServiceType { get; set; }

    public string? CargoDetails { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.New;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
