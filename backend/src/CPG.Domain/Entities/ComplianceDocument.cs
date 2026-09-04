using CPG.Domain.Common;
using CPG.Domain.Enums;

namespace CPG.Domain.Entities;

/// <summary>A single legal document uploaded to blob storage for compliance review (SPEC.md US-03).</summary>
public class ComplianceDocument : Entity, IAuditableEntity
{
    public required Guid CarrierId { get; set; }

    public required ComplianceDocumentType DocumentType { get; set; }

    /// <summary>Absolute URI of the stored blob (Azurite in dev, Azure Blob in prod).</summary>
    public required string BlobUri { get; set; }

    public required string OriginalFileName { get; set; }

    public required string ContentType { get; set; }

    public required long SizeBytes { get; set; }

    public ComplianceStatus Status { get; set; } = ComplianceStatus.UnderReview;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
