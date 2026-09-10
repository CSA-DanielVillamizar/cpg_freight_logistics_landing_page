using CPG.Application.Common.Interfaces;

namespace CPG.Infrastructure.Billing;

/// <summary>
/// Deterministic stand-in for <see cref="StripeConnectService"/> used when
/// <c>Stripe:SecretKey</c> is not configured (T-SDD Epica 2B). Mints fake <c>acct_</c>/<c>tr_</c>
/// ids so the Quick Pay pipeline is exercisable end-to-end without real Stripe credentials.
/// </summary>
public sealed class MockStripeConnectService : IStripeConnectService
{
    public Task<string> CreateConnectedAccountAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult($"acct_mock_{Guid.NewGuid():N}");

    public Task<string> CreateAccountLinkAsync(
        string stripeAccountId, string refreshUrl, string returnUrl, CancellationToken cancellationToken = default)
        => Task.FromResult(
            $"{returnUrl}{(returnUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?")}mockStripeOnboarding={stripeAccountId}");

    public Task<string> CreateTransferAsync(
        string stripeAccountId, decimal amountUsd, string description, CancellationToken cancellationToken = default)
        => Task.FromResult($"tr_mock_{Guid.NewGuid():N}");
}
