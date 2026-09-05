namespace CPG.Application.Features.Telemetry;

/// <summary>
/// A single live telemetry sample for a load in transit, pushed to clients over SignalR
/// (<c>ReceiveTelemetryUpdate</c>).
/// </summary>
public sealed record TelemetryReading
{
    public required Guid LoadId { get; init; }

    public required double Latitude { get; init; }

    public required double Longitude { get; init; }

    /// <summary>Reefer temperature in °C; <c>null</c> for loads without environmental sensors.</summary>
    public double? TemperatureCelsius { get; init; }

    public required int SpeedMph { get; init; }

    public required DateTimeOffset TimestampUtc { get; init; }
}
