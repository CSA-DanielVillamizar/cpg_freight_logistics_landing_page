using CPG.Domain.Enums;

namespace CPG.Application.Features.Compliance;

/// <summary>Result of a successful compliance document upload (SPEC.md US-03).</summary>
public sealed record UploadComplianceDocumentResult
{
    public required Guid CarrierId { get; init; }

    public required Guid DocumentId { get; init; }

    public required ComplianceStatus Status { get; init; }

    public required string BlobUri { get; init; }
}

/// <summary>A single stored compliance document.</summary>
public sealed record ComplianceDocumentSummary
{
    public required Guid Id { get; init; }

    public required ComplianceDocumentType DocumentType { get; init; }

    public required string OriginalFileName { get; init; }

    public required long SizeBytes { get; init; }

    public required ComplianceStatus Status { get; init; }

    public required DateTimeOffset UploadedAtUtc { get; init; }
}

/// <summary>The current carrier's compliance snapshot for the portal.</summary>
public sealed record ComplianceStatusResponse
{
    public required Guid CarrierId { get; init; }

    public required string CompanyName { get; init; }

    public required ComplianceStatus Status { get; init; }

    public required IReadOnlyList<ComplianceDocumentSummary> Documents { get; init; }
}
