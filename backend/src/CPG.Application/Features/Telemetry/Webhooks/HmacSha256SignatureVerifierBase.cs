using System.Security.Cryptography;
using System.Text;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Enums;

namespace CPG.Application.Features.Telemetry.Webhooks;

/// <summary>
/// Shared HMAC-SHA256 (hex-encoded) verification logic. Every current ELD provider uses this
/// same primitive; only the signature header name differs, which subclasses provide — this
/// keeps the Strategy pattern meaningful without duplicating the crypto (T-SDD Epica 2A ADR-03).
/// </summary>
public abstract class HmacSha256SignatureVerifierBase : ITelemetrySignatureVerifier
{
    public abstract TelemetryProvider Provider { get; }

    public abstract string SignatureHeaderName { get; }

    public bool Verify(string rawBody, string? signatureHeaderValue, string webhookSecret)
    {
        if (string.IsNullOrWhiteSpace(signatureHeaderValue) || string.IsNullOrEmpty(webhookSecret))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(ComputeHexSignature(rawBody, webhookSecret));
        var actualBytes = Encoding.UTF8.GetBytes(signatureHeaderValue.Trim());

        // FixedTimeEquals requires equal-length inputs; a length mismatch is itself a safe,
        // non-timing-sensitive rejection.
        return expectedBytes.Length == actualBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }

    private static string ComputeHexSignature(string rawBody, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
