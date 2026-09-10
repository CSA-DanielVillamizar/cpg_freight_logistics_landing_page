using CPG.Domain.Enums;

namespace CPG.Application.Features.Telemetry;

/// <summary>POST /api/carriers/{id}/telemetry-devices request body (T-SDD Epica 2A).</summary>
public sealed record RegisterTelemetryDeviceRequest
{
    public required TelemetryProvider Provider { get; init; }

    /// <summary>Vehicle/device identifier as known by the ELD provider.</summary>
    public required string ExternalDeviceId { get; init; }
}

/// <summary>
/// 201 response — carries the webhook secret in clear text exactly once. The caller must copy
/// it into the ELD provider's webhook configuration; CPG never displays it again.
/// </summary>
public sealed record TelemetryDeviceRegistrationResponse
{
    public required Guid DeviceId { get; init; }

    public required TelemetryProvider Provider { get; init; }

    public required string ExternalDeviceId { get; init; }

    public required string WebhookSecret { get; init; }

    public required string WebhookUrl { get; init; }
}

/// <summary>
/// Canonical webhook payload every provider is configured to send (T-SDD Epica 2A). Real ELD
/// providers use different field names on the wire; a production integration would translate
/// each provider's native schema into this shape before the signature-verified body reaches
/// here. <c>loadId</c> is the CPG load reference the truck is currently hauling.
/// </summary>
public sealed record TelemetryWebhookPayload
{
    public required string DeviceId { get; init; }

    public required Guid LoadId { get; init; }

    public required decimal Latitude { get; init; }

    public required decimal Longitude { get; init; }

    public decimal? SpeedMph { get; init; }

    public decimal? HeadingDegrees { get; init; }

    public required DateTimeOffset RecordedAtUtc { get; init; }
}

/// <summary>GET /api/loads/{id}/location response (T-SDD Epica 2A).</summary>
public sealed record LoadLocationResponse
{
    public required Guid LoadId { get; init; }

    public decimal? Latitude { get; init; }

    public decimal? Longitude { get; init; }

    public DateTimeOffset? LastTelemetryAtUtc { get; init; }
}

/// <summary>A single row of GET /api/loads/{id}/telemetry-history (T-SDD Epica 2A).</summary>
public sealed record TelemetryLogEntryResponse
{
    public required decimal Latitude { get; init; }

    public required decimal Longitude { get; init; }

    public decimal? SpeedMph { get; init; }

    public decimal? HeadingDegrees { get; init; }

    public required DateTimeOffset RecordedAtUtc { get; init; }
}
