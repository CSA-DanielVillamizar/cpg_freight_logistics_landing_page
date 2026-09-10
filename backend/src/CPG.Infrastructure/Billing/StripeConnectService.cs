using CPG.Application.Common.Interfaces;
using Stripe;

namespace CPG.Infrastructure.Billing;

/// <summary>
/// Real Stripe Connect gateway (T-SDD Epica 2B), used when <c>Stripe:SecretKey</c> is
/// configured. Express accounts keep KYC hosted by Stripe — CPG never touches raw bank details.
/// </summary>
public sealed class StripeConnectService(string secretKey) : IStripeConnectService
{
    public async Task<string> CreateConnectedAccountAsync(string email, CancellationToken cancellationToken = default)
    {
        var service = new AccountService(new StripeClient(secretKey));
        var account = await service.CreateAsync(
            new AccountCreateOptions
            {
                Type = "express",
                Email = email,
                Capabilities = new AccountCapabilitiesOptions
                {
                    Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                },
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return account.Id;
    }

    public async Task<string> CreateAccountLinkAsync(
        string stripeAccountId, string refreshUrl, string returnUrl, CancellationToken cancellationToken = default)
    {
        var service = new AccountLinkService(new StripeClient(secretKey));
        var link = await service.CreateAsync(
            new AccountLinkCreateOptions
            {
                Account = stripeAccountId,
                RefreshUrl = refreshUrl,
                ReturnUrl = returnUrl,
                Type = "account_onboarding",
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return link.Url;
    }

    public async Task<string> CreateTransferAsync(
        string stripeAccountId, decimal amountUsd, string description, CancellationToken cancellationToken = default)
    {
        var service = new TransferService(new StripeClient(secretKey));
        var transfer = await service.CreateAsync(
            new TransferCreateOptions
            {
                Amount = (long)Math.Round(amountUsd * 100m, 0, MidpointRounding.AwayFromZero),
                Currency = "usd",
                Destination = stripeAccountId,
                Description = description,
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return transfer.Id;
    }
}
