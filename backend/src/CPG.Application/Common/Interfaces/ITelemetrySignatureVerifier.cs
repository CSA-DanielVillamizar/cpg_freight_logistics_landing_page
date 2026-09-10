using CPG.Domain.Enums;

namespace CPG.Application.Common.Interfaces;

/// <summary>
/// Verifies an inbound ELD webhook's HMAC signature against the shared secret recorded for the
/// device (T-SDD Epica 2A). One implementation per <see cref="TelemetryProvider"/> (Strategy
/// pattern), resolved by <c>TelemetryWebhookSignatureMiddleware</c>.
/// </summary>
public interface ITelemetrySignatureVerifier
{
    TelemetryProvider Provider { get; }

    /// <summary>HTTP header the provider carries its computed signature in.</summary>
    string SignatureHeaderName { get; }

    /// <summary>
    /// Recomputes the expected signature over <paramref name="rawBody"/> using
    /// <paramref name="webhookSecret"/> and compares it to <paramref name="signatureHeaderValue"/>
    /// in constant time.
    /// </summary>
    bool Verify(string rawBody, string? signatureHeaderValue, string webhookSecret);
}
