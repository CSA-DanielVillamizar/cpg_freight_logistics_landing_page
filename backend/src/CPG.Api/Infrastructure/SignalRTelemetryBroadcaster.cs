using CPG.Api.Hubs;
using CPG.Application.Common.Interfaces;
using CPG.Application.Features.Telemetry;
using Microsoft.AspNetCore.SignalR;

namespace CPG.Api.Infrastructure;

/// <summary><see cref="ITelemetryBroadcaster"/> over the SignalR <see cref="TelemetryHub"/>.</summary>
public sealed class SignalRTelemetryBroadcaster(IHubContext<TelemetryHub> hubContext) : ITelemetryBroadcaster
{
    public Task BroadcastAsync(TelemetryReading reading, CancellationToken cancellationToken = default)
        => hubContext.Clients.All.SendAsync(TelemetryHub.ReceiveTelemetryUpdate, reading, cancellationToken);
}
