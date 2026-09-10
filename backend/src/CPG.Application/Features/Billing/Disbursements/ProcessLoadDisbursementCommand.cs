using MediatR;

namespace CPG.Application.Features.Billing.Disbursements;

/// <summary>
/// Prepares (on invoice generation) and settles (on invoice payment) a Carrier's payout for a
/// delivered load (T-SDD Epica 2B). Idempotent and safe to invoke multiple times: it creates
/// the <c>Pending</c> split once, then only attempts the Stripe Connect transfer once the
/// invoice is <c>Paid</c> and the disbursement has not already reached a terminal state.
/// </summary>
public sealed record ProcessLoadDisbursementCommand(Guid InvoiceId) : IRequest;
