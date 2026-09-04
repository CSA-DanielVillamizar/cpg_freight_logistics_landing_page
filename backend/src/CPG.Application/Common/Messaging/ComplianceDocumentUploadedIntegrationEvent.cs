namespace CPG.Application.Common.Messaging;

/// <summary>
/// Raised after a carrier compliance document lands in blob storage and the carrier record
/// transitions to <c>Under Review</c> (SPEC.md US-03).
/// </summary>
public sealed record ComplianceDocumentUploadedIntegrationEvent : IntegrationEvent
{
    public required Guid CarrierId { get; init; }

    public required Guid DocumentId { get; init; }

    public required string DocumentType { get; init; }
}
