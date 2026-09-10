using CPG.Domain.Common;

namespace CPG.Domain.Events;

/// <summary>Raised when a <see cref="Entities.PaymentDisbursement"/> is transferred successfully via Stripe Connect (T-SDD Epica 2B).</summary>
public sealed record PaymentDisbursementCompletedDomainEvent(
    Guid DisbursementId,
    Guid LoadId,
    Guid CarrierId,
    decimal CarrierNetAmountUsd) : DomainEvent;
