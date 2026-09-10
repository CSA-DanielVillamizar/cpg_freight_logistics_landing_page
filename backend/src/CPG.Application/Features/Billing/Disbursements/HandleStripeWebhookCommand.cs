using MediatR;

namespace CPG.Application.Features.Billing.Disbursements;

/// <summary>
/// Reconciles a Stripe Connect transfer.paid/transfer.failed webhook against the matching
/// PaymentDisbursement row (T-SDD Epica 2B). The signature is already verified by the
/// controller before this command runs.
/// </summary>
public sealed record HandleStripeWebhookCommand(string EventType, string StripeTransferId, string? FailureReason)
    : IRequest;
