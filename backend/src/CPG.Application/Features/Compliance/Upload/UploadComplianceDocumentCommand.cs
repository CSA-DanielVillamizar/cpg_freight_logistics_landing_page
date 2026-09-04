using CPG.Domain.Enums;
using MediatR;

namespace CPG.Application.Features.Compliance.Upload;

/// <summary>
/// Uploads a carrier compliance document (SPEC.md US-03). The carrier is resolved from the
/// authenticated principal - never from the request payload.
/// </summary>
public sealed record UploadComplianceDocumentCommand : IRequest<UploadComplianceDocumentResult>
{
    public required ComplianceDocumentType DocumentType { get; init; }

    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required long SizeBytes { get; init; }

    /// <summary>Caller-owned stream positioned at the start of the file content.</summary>
    public required Stream Content { get; init; }
}
