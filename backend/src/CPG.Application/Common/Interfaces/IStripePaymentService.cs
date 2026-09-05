namespace CPG.Application.Common.Interfaces;

/// <summary>Inputs for opening a Stripe Checkout session for one invoice.</summary>
public sealed record StripeCheckoutRequest
{
    public required Guid InvoiceId { get; init; }

    public required string InvoiceReference { get; init; }

    public required decimal AmountUsd { get; init; }

    public required string CustomerEmail { get; init; }

    public required string SuccessUrl { get; init; }

    public required string CancelUrl { get; init; }
}

/// <summary>A hosted Stripe Checkout session (real or mocked).</summary>
public sealed record StripeCheckoutSession(string SessionId, string CheckoutUrl);

/// <summary>The outcome of validating and parsing a Stripe webhook payload.</summary>
public sealed record StripeWebhookResult(bool SignatureValid, bool CheckoutCompleted, string? SessionId)
{
    public static StripeWebhookResult Invalid { get; } = new(false, false, null);
}

/// <summary>
/// Payment gateway abstraction. Backed by <c>Stripe.net</c> when <c>Stripe:SecretKey</c> is
/// configured; otherwise a deterministic mock that drives the same Checkout -&gt; webhook flow.
/// </summary>
public interface IStripePaymentService
{
    Task<StripeCheckoutSession> CreateCheckoutSessionAsync(
        StripeCheckoutRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Validates the signature header and reports whether it is a completed checkout.</summary>
    StripeWebhookResult HandleWebhook(string payload, string? signatureHeader);
}
