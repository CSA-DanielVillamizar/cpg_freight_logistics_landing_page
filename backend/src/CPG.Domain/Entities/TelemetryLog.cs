using CPG.Domain.Common;

namespace CPG.Domain.Entities;

/// <summary>An append-only GPS reading ingested from an ELD webhook (T-SDD Epica 2A).</summary>
public class TelemetryLog : Entity
{
    public required Guid LoadId { get; set; }

    public required Guid TelemetryDeviceId { get; set; }

    public required decimal Latitude { get; set; }

    public required decimal Longitude { get; set; }

    public decimal? SpeedMph { get; set; }

    public decimal? HeadingDegrees { get; set; }

    /// <summary>Timestamp reported by the ELD hardware.</summary>
    public required DateTimeOffset RecordedAtUtc { get; set; }

    /// <summary>Timestamp the CPG platform ingested the webhook.</summary>
    public DateTimeOffset ReceivedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
