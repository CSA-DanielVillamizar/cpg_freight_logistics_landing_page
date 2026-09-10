using CPG.Domain.Common;
using CPG.Domain.Enums;

namespace CPG.Domain.Entities;

/// <summary>An ELD hardware unit registered against a Carrier for authenticated telemetry webhooks (T-SDD Epica 2A).</summary>
public class TelemetryDevice : Entity, IAuditableEntity
{
    public required Guid CarrierId { get; set; }

    public required TelemetryProvider Provider { get; set; }

    /// <summary>Vehicle/device identifier as known by the ELD provider.</summary>
    public required string ExternalDeviceId { get; set; }

    /// <summary>Hashed HMAC webhook secret; never stored or logged in clear text.</summary>
    public required string WebhookSecretHash { get; set; }

    public DateTimeOffset? LastSeenAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
