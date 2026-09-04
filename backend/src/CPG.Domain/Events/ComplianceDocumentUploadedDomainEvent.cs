using CPG.Domain.Common;
using CPG.Domain.Enums;

namespace CPG.Domain.Events;

/// <summary>
/// Raised when a carrier files a compliance document and the account moves to
/// <see cref="ComplianceStatus.UnderReview"/> (SPEC.md US-03). Dispatched after the
/// PostgreSQL transaction commits; a handler forwards it to RabbitMQ.
/// </summary>
public sealed record ComplianceDocumentUploadedDomainEvent(
    Guid CarrierId,
    Guid DocumentId,
    ComplianceDocumentType DocumentType) : DomainEvent;
