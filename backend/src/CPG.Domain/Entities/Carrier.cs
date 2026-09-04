using CPG.Domain.Common;
using CPG.Domain.Enums;

namespace CPG.Domain.Entities;

/// <summary>A freight carrier / owner-operator account (SPEC.md US-03).</summary>
public class Carrier : AggregateRoot, IAuditableEntity, IHasRowVersion
{
    public required string CompanyName { get; set; }

    public required Guid UserId { get; set; }

    public string? DotNumber { get; set; }

    public string? McNumber { get; set; }

    public ComplianceStatus ComplianceStatus { get; set; } = ComplianceStatus.PendingCompliance;

    public ICollection<ComplianceDocument> ComplianceDocuments { get; } = [];

    /// <summary>Optimistic concurrency token mapped to PostgreSQL <c>xmin</c>.</summary>
    public uint RowVersion { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
