using CPG.Domain.Common;
using CPG.Domain.Enums;

namespace CPG.Domain.Entities;

/// <summary>
/// A freight invoice raised against a delivered <see cref="Load"/> and billed to the corporate
/// shipper that requested it. Paid through Stripe Checkout.
/// </summary>
public class Invoice : AggregateRoot, IAuditableEntity, IHasRowVersion
{
    public required string Reference { get; set; }

    public required Guid LoadId { get; set; }

    public required Guid ShipperUserId { get; set; }

    public required decimal AmountUsd { get; set; }

    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;

    public required DateTimeOffset IssuedAtUtc { get; set; }

    public required DateTimeOffset DueDate { get; set; }

    public string? StripeSessionId { get; private set; }

    public string? StripeCheckoutUrl { get; private set; }

    public DateTimeOffset? PaidAtUtc { get; private set; }

    /// <summary>Optimistic concurrency token mapped to PostgreSQL <c>xmin</c>.</summary>
    public uint RowVersion { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }

    /// <summary>Creates a <c>Pending</c> invoice for a load that has just been delivered.</summary>
    public static Invoice ForDeliveredLoad(Load load, string reference, DateTimeOffset issuedAtUtc, int netDays = 30)
    {
        ArgumentNullException.ThrowIfNull(load);

        if (load.ShipperUserId is not { } shipperUserId)
        {
            throw new DomainException($"Load {load.Reference} has no shipper to bill.");
        }

        return new Invoice
        {
            Reference = reference,
            LoadId = load.Id,
            ShipperUserId = shipperUserId,
            AmountUsd = load.RateUsd,
            Status = InvoiceStatus.Pending,
            IssuedAtUtc = issuedAtUtc,
            DueDate = issuedAtUtc.AddDays(netDays),
        };
    }

    /// <summary>Records the Stripe Checkout session opened for this invoice.</summary>
    public void AttachCheckoutSession(string sessionId, string checkoutUrl)
    {
        if (Status == InvoiceStatus.Paid)
        {
            throw new DomainException($"Invoice {Reference} is already paid.");
        }

        StripeSessionId = sessionId;
        StripeCheckoutUrl = checkoutUrl;
    }

    /// <summary>Marks the invoice paid (idempotent — a re-delivered webhook is a no-op).</summary>
    public void MarkPaid(DateTimeOffset paidAtUtc)
    {
        if (Status == InvoiceStatus.Paid)
        {
            return;
        }

        Status = InvoiceStatus.Paid;
        PaidAtUtc = paidAtUtc;
    }
}
