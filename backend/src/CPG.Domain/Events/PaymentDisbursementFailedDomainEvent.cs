using CPG.Domain.Common;

namespace CPG.Domain.Events;

/// <summary>Raised when a <see cref="Entities.PaymentDisbursement"/> cannot be transferred via Stripe Connect (T-SDD Epica 2B).</summary>
public sealed record PaymentDisbursementFailedDomainEvent(
    Guid DisbursementId,
    Guid LoadId,
    Guid CarrierId,
    string Reason) : DomainEvent;
