using CPG.Application.Features.Telemetry;

namespace CPG.Application.Common.Interfaces;

/// <summary>
/// Pushes a telemetry sample to connected tracking clients. Implemented in the API layer over
/// SignalR; consumed by the infrastructure fleet simulator and the real ELD webhook pipeline.
/// </summary>
public interface ITelemetryBroadcaster
{
    /// <summary>Broadcasts to every connected client (used by the simulator's fleet-wide demo feed).</summary>
    Task BroadcastAsync(TelemetryReading reading, CancellationToken cancellationToken = default);

    /// <summary>Broadcasts only to clients that joined the <c>load-{loadId}</c> group (T-SDD Epica 2A).</summary>
    Task BroadcastToLoadGroupAsync(Guid loadId, TelemetryReading reading, CancellationToken cancellationToken = default);
}
