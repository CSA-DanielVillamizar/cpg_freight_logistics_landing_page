using CPG.Application.Features.Telemetry;

namespace CPG.Application.Common.Interfaces;

/// <summary>
/// Pushes a telemetry sample to connected tracking clients. Implemented in the API layer over
/// SignalR; consumed by the infrastructure fleet simulator.
/// </summary>
public interface ITelemetryBroadcaster
{
    Task BroadcastAsync(TelemetryReading reading, CancellationToken cancellationToken = default);
}
