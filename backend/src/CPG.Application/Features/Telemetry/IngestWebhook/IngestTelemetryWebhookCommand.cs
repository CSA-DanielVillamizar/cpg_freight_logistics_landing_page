using CPG.Domain.Enums;
using MediatR;

namespace CPG.Application.Features.Telemetry.IngestWebhook;

/// <summary>
/// Ingests one verified GPS reading from an ELD webhook (T-SDD Epica 2A). The signature is
/// already validated by <c>TelemetryWebhookSignatureMiddleware</c> before this command runs.
/// </summary>
public sealed record IngestTelemetryWebhookCommand(
    TelemetryProvider Provider,
    string ExternalDeviceId,
    Guid LoadId,
    decimal Latitude,
    decimal Longitude,
    decimal? SpeedMph,
    decimal? HeadingDegrees,
    DateTimeOffset RecordedAtUtc) : IRequest
{
    public static IngestTelemetryWebhookCommand FromPayload(TelemetryProvider provider, TelemetryWebhookPayload payload) =>
        new(
            provider,
            payload.DeviceId,
            payload.LoadId,
            payload.Latitude,
            payload.Longitude,
            payload.SpeedMph,
            payload.HeadingDegrees,
            payload.RecordedAtUtc);
}
