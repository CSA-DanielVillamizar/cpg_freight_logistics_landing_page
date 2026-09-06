using CPG.Api.Infrastructure;
using CPG.Application.Features.Carriers;
using CPG.Application.Features.Carriers.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Carrier self-service onboarding. Restricted to the <c>Carrier</c> role.</summary>
[Authorize(Policy = AuthorizationPolicies.CarrierOnly)]
public sealed class CarriersController(ISender sender) : ApiControllerBase
{
    /// <summary>
    /// Creates the authenticated carrier user's profile so they can accept loads and file
    /// compliance documents. One profile per account; the account starts
    /// <c>PendingCompliance</c>. Returns 409 if a profile already exists.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CarrierRegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CarrierRegistrationResponse>> Register(
        [FromBody] RegisterCarrierRequest request,
        CancellationToken cancellationToken)
    {
        var carrier = await sender.Send(RegisterCarrierCommand.FromRequest(request), cancellationToken);
        return Created($"/api/carriers/{carrier.CarrierId}", carrier);
    }
}
