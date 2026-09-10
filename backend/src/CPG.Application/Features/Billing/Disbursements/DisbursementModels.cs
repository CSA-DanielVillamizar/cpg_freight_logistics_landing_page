using CPG.Domain.Enums;

namespace CPG.Application.Features.Billing.Disbursements;

/// <summary>POST /api/carriers/stripe-connect 201 response (T-SDD Epica 2B).</summary>
public sealed record StripeConnectOnboardingResponse
{
    public required string AccountLinkUrl { get; init; }
}

/// <summary>A single row of GET /api/carriers/payouts (T-SDD Epica 2B).</summary>
public sealed record PayoutHistoryEntryResponse
{
    public required Guid DisbursementId { get; init; }

    public required Guid InvoiceId { get; init; }

    public required string LoadReference { get; init; }

    public required decimal GrossAmountUsd { get; init; }

    public required decimal CpgMarginAmountUsd { get; init; }

    public required decimal AgentCommissionAmountUsd { get; init; }

    public required decimal QuickPayFeeAmountUsd { get; init; }

    public required decimal CarrierNetAmountUsd { get; init; }

    public required bool QuickPayRequested { get; init; }

    public required DisbursementStatus Status { get; init; }

    public string? FailureReason { get; init; }

    public DateTimeOffset? ProcessedAtUtc { get; init; }

    public required DateTimeOffset CreatedAtUtc { get; init; }
}
