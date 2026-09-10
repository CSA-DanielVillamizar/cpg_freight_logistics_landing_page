using MediatR;

namespace CPG.Application.Features.Billing.Disbursements;

/// <summary>
/// A Carrier begins (or resumes) Stripe Connect onboarding so they can receive payouts
/// (T-SDD Epica 2B). Returns a hosted onboarding link — CPG never collects KYC data directly.
/// </summary>
public sealed record CreateStripeConnectAccountCommand(string RefreshUrl, string ReturnUrl)
    : IRequest<StripeConnectOnboardingResponse>;
