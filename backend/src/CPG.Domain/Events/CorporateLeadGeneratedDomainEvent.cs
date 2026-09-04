using CPG.Domain.Common;
using CPG.Domain.Enums;

namespace CPG.Domain.Events;

/// <summary>
/// Raised when a niche vertical landing page captures a qualified enterprise lead
/// (SPEC.md US-04). Dispatched after the PostgreSQL insert commits; a handler forwards it
/// to RabbitMQ so the commercial team is notified.
/// </summary>
public sealed record CorporateLeadGeneratedDomainEvent(
    Guid LeadId,
    string CompanyName,
    string ContactEmail,
    string VerticalSlug,
    ServiceType? ServiceType) : DomainEvent;
