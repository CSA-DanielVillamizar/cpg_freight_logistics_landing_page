namespace CPG.Application.Common.Messaging;

/// <summary>
/// Raised after a landing-page lead is persisted with status <c>New</c>, so the commercial
/// team can be notified asynchronously (SPEC.md US-04).
/// </summary>
public sealed record LeadCreatedIntegrationEvent : IntegrationEvent
{
    public required Guid LeadId { get; init; }

    public required string CompanyName { get; init; }

    public required string ContactEmail { get; init; }

    public required string VerticalSlug { get; init; }
}
