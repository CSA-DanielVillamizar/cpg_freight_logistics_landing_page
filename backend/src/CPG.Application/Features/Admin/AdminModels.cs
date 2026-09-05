using CPG.Domain.Enums;

namespace CPG.Application.Features.Admin;

/// <summary>A carrier account and its compliance documents, for the admin control tower.</summary>
public sealed record CarrierComplianceView
{
    public required Guid Id { get; init; }

    public required string CompanyName { get; init; }

    public string? DotNumber { get; init; }

    public string? McNumber { get; init; }

    public required ComplianceStatus Status { get; init; }

    /// <summary>When the carrier last submitted a document for review (max upload time).</summary>
    public DateTimeOffset? SubmittedAtUtc { get; init; }

    public DateTimeOffset? LastReviewedAtUtc { get; init; }

    public required IReadOnlyList<CarrierDocumentView> Documents { get; init; }
}

/// <summary>A single stored compliance document as seen by an administrator.</summary>
public sealed record CarrierDocumentView
{
    public required Guid Id { get; init; }

    public required ComplianceDocumentType DocumentType { get; init; }

    public required string OriginalFileName { get; init; }

    public required string ContentType { get; init; }

    public required long SizeBytes { get; init; }

    public required ComplianceStatus Status { get; init; }

    public required DateTimeOffset UploadedAtUtc { get; init; }
}

/// <summary>The bytes of a compliance document, streamed back to an authorised admin.</summary>
public sealed record CarrierDocumentContent
{
    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required Stream Content { get; init; }
}

/// <summary>Outcome of an administrative compliance review.</summary>
public enum ReviewDecision
{
    Approve = 1,
    Reject = 2,
}
