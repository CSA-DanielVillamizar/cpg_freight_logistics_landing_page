using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CPG.Api.Hubs;

/// <summary>
/// Real-time fleet telemetry channel. Server-push only — the infrastructure fleet simulator
/// sends <c>ReceiveTelemetryUpdate</c> messages via <c>IHubContext</c>. Requires a valid JWT;
/// over WebSockets the token arrives in the <c>access_token</c> query string.
/// </summary>
[Authorize]
public sealed class TelemetryHub : Hub
{
    /// <summary>Client-side handler name for a <c>TelemetryReading</c> payload.</summary>
    public const string ReceiveTelemetryUpdate = "ReceiveTelemetryUpdate";
}
