using System.Globalization;
using System.Text.Json;
using CPG.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CPG.Infrastructure.Billing;

/// <summary>
/// Deterministic stand-in for Stripe used when <c>Stripe:SecretKey</c> is not configured. It
/// mints a <c>cs_mock_*</c> session id and points the browser at the API's own mock Checkout
/// page, which drives exactly the same <c>MarkInvoicePaidCommand</c> path a real webhook would.
/// Swapping in <c>Stripe.net</c> means replacing this one class.
/// </summary>
public sealed class MockStripePaymentService(IConfiguration configuration) : IStripePaymentService
{
    private const string DefaultWebhookSecret = "whsec_cpg_mock";
    private const string DefaultCheckoutBaseUrl = "http://localhost:5080/api/billing/mock-checkout";

    public Task<StripeCheckoutSession> CreateCheckoutSessionAsync(
        StripeCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var baseUrl = configuration["Billing:MockCheckoutBaseUrl"] ?? DefaultCheckoutBaseUrl;
        var sessionId = $"cs_mock_{Guid.NewGuid():N}";

        var checkoutUrl =
            $"{baseUrl.TrimEnd('/')}/{sessionId}" +
            $"?amount={request.AmountUsd.ToString("F2", CultureInfo.InvariantCulture)}" +
            $"&reference={Uri.EscapeDataString(request.InvoiceReference)}" +
            $"&success={Uri.EscapeDataString(request.SuccessUrl)}" +
            $"&cancel={Uri.EscapeDataString(request.CancelUrl)}";

        return Task.FromResult(new StripeCheckoutSession(sessionId, checkoutUrl));
    }

    public StripeWebhookResult HandleWebhook(string payload, string? signatureHeader)
    {
        var expectedSecret = configuration["Stripe:WebhookSecret"] ?? DefaultWebhookSecret;

        if (string.IsNullOrEmpty(signatureHeader)
            || !string.Equals(signatureHeader, expectedSecret, StringComparison.Ordinal))
        {
            return StripeWebhookResult.Invalid;
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            var type = root.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;
            var sessionId = root.TryGetProperty("data", out var data)
                && data.TryGetProperty("object", out var obj)
                && obj.TryGetProperty("id", out var id)
                ? id.GetString()
                : null;

            return new StripeWebhookResult(
                SignatureValid: true,
                CheckoutCompleted: string.Equals(type, "checkout.session.completed", StringComparison.Ordinal),
                SessionId: sessionId);
        }
        catch (JsonException)
        {
            return new StripeWebhookResult(SignatureValid: true, CheckoutCompleted: false, SessionId: null);
        }
    }
}
