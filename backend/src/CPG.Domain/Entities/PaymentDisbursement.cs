using CPG.Domain.Common;
using CPG.Domain.Enums;
using CPG.Domain.Events;

namespace CPG.Domain.Entities;

/// <summary>
/// An append-only ledger row recording the split of a delivered load's revenue between CPG's
/// margin, an optional Agent's commission, an optional Quick Pay fee and the Carrier's net
/// payout (T-SDD Epica 2B). Rows are never mutated once <see cref="Status"/> leaves
/// <see cref="DisbursementStatus.Pending"/>/<see cref="DisbursementStatus.Processing"/>;
/// corrections are new rows, not edits.
/// </summary>
public class PaymentDisbursement : AggregateRoot, IAuditableEntity
{
    public required Guid LoadId { get; set; }

    public required Guid InvoiceId { get; set; }

    public required Guid CarrierId { get; set; }

    /// <summary>Null when the load was not published by an Independent Agent.</summary>
    public Guid? AgentId { get; set; }

    public required decimal GrossAmountUsd { get; set; }

    public required decimal CpgMarginAmountUsd { get; set; }

    public decimal AgentCommissionAmountUsd { get; set; }

    public decimal QuickPayFeeAmountUsd { get; set; }

    public required decimal CarrierNetAmountUsd { get; set; }

    public bool QuickPayRequested { get; set; }

    public DisbursementStatus Status { get; private set; } = DisbursementStatus.Pending;

    public string? StripeTransferId { get; private set; }

    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }

    /// <summary>Records the Stripe Connect transfer id while the payout settles (T-SDD Epica 2B).</summary>
    /// <exception cref="DomainException">The disbursement already reached a terminal state.</exception>
    public void MarkTransferInitiated(string stripeTransferId)
    {
        if (Status is DisbursementStatus.Completed or DisbursementStatus.Failed)
        {
            throw new DomainException($"Payment disbursement {Id} cannot restart a transfer from status {Status}.");
        }

        StripeTransferId = stripeTransferId;
        Status = DisbursementStatus.Processing;
    }

    /// <summary>Marks the disbursement as transferred to the Carrier's Stripe Connect account.</summary>
    /// <exception cref="DomainException">The disbursement has already completed.</exception>
    public void MarkCompleted(string stripeTransferId, DateTimeOffset processedAtUtc)
    {
        if (Status == DisbursementStatus.Completed)
        {
            throw new DomainException($"Payment disbursement {Id} is already completed.");
        }

        StripeTransferId = stripeTransferId;
        ProcessedAtUtc = processedAtUtc;
        Status = DisbursementStatus.Completed;

        RaiseDomainEvent(new PaymentDisbursementCompletedDomainEvent(
            Id, LoadId, CarrierId, CarrierNetAmountUsd, stripeTransferId, QuickPayRequested));
    }

    /// <summary>Marks the disbursement as failed (e.g. the Carrier has no connected Stripe account).</summary>
    /// <exception cref="DomainException">The disbursement has already completed.</exception>
    public void MarkFailed(string reason)
    {
        if (Status == DisbursementStatus.Completed)
        {
            throw new DomainException($"Payment disbursement {Id} cannot fail after completion.");
        }

        FailureReason = reason;
        Status = DisbursementStatus.Failed;

        RaiseDomainEvent(new PaymentDisbursementFailedDomainEvent(Id, LoadId, CarrierId, reason));
    }
}
