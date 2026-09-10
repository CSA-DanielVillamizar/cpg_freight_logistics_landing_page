using CPG.Application.Common.Interfaces;
using CPG.Application.Features.Telemetry.Webhooks;
using CPG.Domain.Enums;

namespace CPG.Application.UnitTests;

public sealed class TelemetrySignatureVerifierTests
{
    private const string RawBody = """{"deviceId":"TRUCK-42","loadId":"1c2d3e4f-0000-0000-0000-000000000001","latitude":28.5,"longitude":-81.3,"recordedAtUtc":"2026-09-10T12:00:00Z"}""";
    private const string Secret = "correct-horse-battery-staple";

    public static TheoryData<ITelemetrySignatureVerifier, TelemetryProvider, string> Verifiers() => new()
    {
        { new SamsaraSignatureVerifier(), TelemetryProvider.Samsara, "X-Samsara-Signature" },
        { new MotiveSignatureVerifier(), TelemetryProvider.Motive, "X-Motive-Signature" },
        { new KeepTruckinSignatureVerifier(), TelemetryProvider.KeepTruckin, "X-KeepTruckin-Signature" },
    };

    public static TheoryData<ITelemetrySignatureVerifier> VerifierInstances() => new()
    {
        new SamsaraSignatureVerifier(),
        new MotiveSignatureVerifier(),
        new KeepTruckinSignatureVerifier(),
    };

    [Theory]
    [MemberData(nameof(Verifiers))]
    public void Verify_accepts_a_correctly_computed_signature(
        ITelemetrySignatureVerifier verifier, TelemetryProvider expectedProvider, string expectedHeaderName)
    {
        Assert.Equal(expectedProvider, verifier.Provider);
        Assert.Equal(expectedHeaderName, verifier.SignatureHeaderName);

        var validSignature = ComputeHexHmac(RawBody, Secret);
        Assert.True(verifier.Verify(RawBody, validSignature, Secret));
    }

    [Theory]
    [MemberData(nameof(VerifierInstances))]
    public void Verify_rejects_a_tampered_signature(ITelemetrySignatureVerifier verifier)
    {
        var tamperedSignature = ComputeHexHmac(RawBody, "wrong-secret");
        Assert.False(verifier.Verify(RawBody, tamperedSignature, Secret));
    }

    [Theory]
    [MemberData(nameof(VerifierInstances))]
    public void Verify_rejects_a_missing_signature_header(ITelemetrySignatureVerifier verifier)
    {
        Assert.False(verifier.Verify(RawBody, null, Secret));
        Assert.False(verifier.Verify(RawBody, string.Empty, Secret));
    }

    private static string ComputeHexHmac(string body, string secret)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(body));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
