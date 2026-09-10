using System.Security.Claims;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Api.Hubs;

/// <summary>
/// Real-time fleet telemetry channel. Requires a valid JWT; over WebSockets the token arrives
/// in the <c>access_token</c> query string. Two delivery modes:
/// <list type="bullet">
/// <item><description>Fleet-wide demo feed — <c>Clients.All</c> (the simulator).</description></item>
/// <item><description>
/// Per-load group <c>load-{loadId}</c> — real ELD webhooks (T-SDD Epica 2A). Clients must call
/// <see cref="JoinLoadGroupAsync"/>, which is authorized against the load's Shipper.
/// </description></item>
/// </list>
/// </summary>
[Authorize]
public sealed class TelemetryHub(IApplicationDbContext dbContext) : Hub
{
    /// <summary>Client-side handler name for a <c>TelemetryReading</c> payload.</summary>
    public const string ReceiveTelemetryUpdate = "ReceiveTelemetryUpdate";

    public static string LoadGroupName(Guid loadId) => $"load-{loadId}";

    /// <summary>
    /// Joins the caller's connection to <c>load-{loadId}</c> so it receives that load's GPS
    /// updates. Only the load's Shipper or an Admin may join.
    /// </summary>
    /// <exception cref="HubException">The caller is not authorized to track this load.</exception>
    public async Task JoinLoadGroupAsync(Guid loadId)
    {
        if (!await CanTrackLoadAsync(loadId).ConfigureAwait(false))
        {
            throw new HubException("You are not authorized to track this load.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, LoadGroupName(loadId)).ConfigureAwait(false);
    }

    public Task LeaveLoadGroupAsync(Guid loadId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, LoadGroupName(loadId));

    private async Task<bool> CanTrackLoadAsync(Guid loadId)
    {
        var roleClaim = Context.User?.FindFirstValue(ClaimTypes.Role);
        if (Enum.TryParse<UserRole>(roleClaim, out var role) && role == UserRole.Admin)
        {
            return true;
        }

        var subClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");
        if (!Guid.TryParse(subClaim, out var userId))
        {
            return false;
        }

        var shipperUserId = await dbContext.Loads
            .AsNoTracking()
            .Where(load => load.Id == loadId)
            .Select(load => load.ShipperUserId)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        return shipperUserId == userId;
    }
}
