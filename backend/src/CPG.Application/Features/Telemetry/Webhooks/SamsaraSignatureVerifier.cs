using CPG.Domain.Enums;

namespace CPG.Application.Features.Telemetry.Webhooks;

/// <summary>Samsara ELD webhook signature strategy (T-SDD Epica 2A).</summary>
public sealed class SamsaraSignatureVerifier : HmacSha256SignatureVerifierBase
{
    public override TelemetryProvider Provider => TelemetryProvider.Samsara;

    public override string SignatureHeaderName => "X-Samsara-Signature";
}
