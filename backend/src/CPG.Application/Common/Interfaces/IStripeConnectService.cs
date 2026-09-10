namespace CPG.Application.Common.Interfaces;

/// <summary>
/// Stripe Connect gateway abstraction (T-SDD Epica 2B). Backed by <c>Stripe.net</c> when
/// <c>Stripe:SecretKey</c> is configured; otherwise a deterministic mock so the Quick Pay
/// pipeline is exercisable end-to-end without real Stripe credentials.
/// </summary>
public interface IStripeConnectService
{
    /// <summary>Creates a Stripe Express connected account for a Carrier/Agent onboarding for payouts.</summary>
    Task<string> CreateConnectedAccountAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Creates a hosted onboarding link for an existing connected account.</summary>
    Task<string> CreateAccountLinkAsync(
        string stripeAccountId,
        string refreshUrl,
        string returnUrl,
        CancellationToken cancellationToken = default);

    /// <summary>Transfers funds from the platform balance to a connected account. Returns the Stripe transfer id.</summary>
    Task<string> CreateTransferAsync(
        string stripeAccountId,
        decimal amountUsd,
        string description,
        CancellationToken cancellationToken = default);
}
