using CPG.Domain.Enums;

namespace CPG.Application.Features.Telemetry.Webhooks;

/// <summary>KeepTruckin (Motive's legacy brand) ELD webhook signature strategy (T-SDD Epica 2A).</summary>
public sealed class KeepTruckinSignatureVerifier : HmacSha256SignatureVerifierBase
{
    public override TelemetryProvider Provider => TelemetryProvider.KeepTruckin;

    public override string SignatureHeaderName => "X-KeepTruckin-Signature";
}
