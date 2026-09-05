using CPG.Api.Infrastructure;
using CPG.Application.Features.Loads;
using CPG.Application.Features.Loads.Accept;
using CPG.Application.Features.Loads.Deliver;
using CPG.Application.Features.Loads.GetLoads;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Carrier &amp; Shipper Load Workspace — board listing and load assignment.</summary>
[Authorize]
public sealed class LoadsController(ISender sender) : ApiControllerBase
{
    /// <summary>
    /// The load board, newest pickup first. Optional filters: <c>status</c> / <c>serviceType</c>
    /// (repeatable), <c>origin</c>, <c>destination</c>. Any authenticated user may read it.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LoadSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<LoadSummaryResponse>>> GetLoads(
        [FromQuery(Name = "status")] LoadStatus[]? status,
        [FromQuery(Name = "serviceType")] ServiceType[]? serviceType,
        [FromQuery(Name = "origin")] string? origin,
        [FromQuery(Name = "destination")] string? destination,
        CancellationToken cancellationToken)
        => Ok(await sender.Send(
            new GetLoadsQuery(status, serviceType, origin, destination),
            cancellationToken));

    /// <summary>
    /// The authenticated carrier accepts an available load; it transitions to <c>Dispatched</c>.
    /// Returns 409 if the load is no longer available. Restricted to the <c>Carrier</c> role.
    /// </summary>
    [HttpPost("{id:guid}/accept")]
    [Authorize(Policy = AuthorizationPolicies.CarrierOnly)]
    [ProducesResponseType(typeof(LoadSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoadSummaryResponse>> AcceptLoad(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await sender.Send(new AcceptLoadCommand(id), cancellationToken));

    /// <summary>
    /// The assigned carrier marks their in-transit load delivered; billing then raises the
    /// shipper invoice asynchronously. 403 if assigned to another carrier, 409 if not in transit.
    /// </summary>
    [HttpPost("{id:guid}/deliver")]
    [Authorize(Policy = AuthorizationPolicies.CarrierOnly)]
    [ProducesResponseType(typeof(LoadSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoadSummaryResponse>> DeliverLoad(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await sender.Send(new MarkLoadDeliveredCommand(id), cancellationToken));
}
