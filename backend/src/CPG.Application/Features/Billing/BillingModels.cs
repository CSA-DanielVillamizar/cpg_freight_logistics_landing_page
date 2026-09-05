using CPG.Domain.Enums;

namespace CPG.Application.Features.Billing;

/// <summary>A single freight invoice as seen by the shipper it is billed to.</summary>
public sealed record ShipperInvoiceView
{
    public required Guid Id { get; init; }

    public required string Reference { get; init; }

    public required string LoadReference { get; init; }

    public required decimal AmountUsd { get; init; }

    /// <summary>Effective status — a past-due <c>Pending</c> invoice is reported as <c>Overdue</c>.</summary>
    public required InvoiceStatus Status { get; init; }

    public required DateTimeOffset IssuedAtUtc { get; init; }

    public required DateTimeOffset DueDate { get; init; }

    public DateTimeOffset? PaidAtUtc { get; init; }

    public required bool Payable { get; init; }
}

/// <summary>The shipper billing payload: invoices plus the outstanding balance.</summary>
public sealed record ShipperInvoicesResponse
{
    public required IReadOnlyList<ShipperInvoiceView> Invoices { get; init; }

    public required decimal TotalOutstandingUsd { get; init; }

    public required int OverdueCount { get; init; }
}

/// <summary>The Checkout URL the shipper's browser should be redirected to.</summary>
public sealed record InvoiceCheckoutResponse
{
    public required string CheckoutUrl { get; init; }
}
