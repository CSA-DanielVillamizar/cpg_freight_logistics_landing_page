namespace CPG.Domain.Common;

/// <summary>Marks an aggregate that can receive payouts via a Stripe Connect connected account.</summary>
public interface IHasStripeConnectAccount
{
    string? StripeConnectAccountId { get; set; }
}
