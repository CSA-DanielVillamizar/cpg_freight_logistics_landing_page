using CPG.Api.Infrastructure;
using CPG.Application.Features.Shipper;
using CPG.Application.Features.Shipper.GetLoadPod;
using CPG.Application.Features.Shipper.GetShipperLoads;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Corporate shipper self-service portal. Restricted to the <c>Shipper</c> role.</summary>
[Authorize(Policy = AuthorizationPolicies.ShipperOnly)]
[Route("api/shipper")]
public sealed class ShipperController(ISender sender) : ApiControllerBase
{
    /// <summary>
    /// The authenticated shipper's loads, split into active shipments (Dispatched / InTransit)
    /// and delivered history, plus headline metrics.
    /// </summary>
    [HttpGet("loads")]
    [ProducesResponseType(typeof(ShipperLoadsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ShipperLoadsResponse>> GetLoads(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetShipperLoadsQuery(), cancellationToken));

    /// <summary>
    /// Downloads the proof-of-delivery PDF for one of the shipper's delivered loads. Returns 403
    /// if the load belongs to another shipper, 404 if it does not exist, 409 if it is not yet
    /// delivered / has no POD.
    /// </summary>
    [HttpGet("loads/{id:guid}/pod")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetPod(Guid id, CancellationToken cancellationToken)
    {
        var pod = await sender.Send(new GetLoadPodQuery(id), cancellationToken);
        return File(pod.Content, pod.ContentType, pod.FileName);
    }
}
