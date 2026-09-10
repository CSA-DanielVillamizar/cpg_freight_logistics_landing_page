using CPG.Domain.Enums;

namespace CPG.Application.Features.Telemetry.Webhooks;

/// <summary>Motive ELD webhook signature strategy (T-SDD Epica 2A).</summary>
public sealed class MotiveSignatureVerifier : HmacSha256SignatureVerifierBase
{
    public override TelemetryProvider Provider => TelemetryProvider.Motive;

    public override string SignatureHeaderName => "X-Motive-Signature";
}
