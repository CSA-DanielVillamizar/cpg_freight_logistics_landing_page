using CPG.Application.Features.Leads;
using CPG.Application.Features.Leads.Create;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Niche landing-page lead capture (SPEC.md US-04). Public - no JWT required.</summary>
[AllowAnonymous]
public sealed class LeadsController(ISender sender) : ApiControllerBase
{
    /// <summary>
    /// Submit an enterprise inquiry. On success the lead is persisted with status <c>New</c>
    /// and a <c>CorporateLeadGenerated</c> event is published to RabbitMQ (SPEC.md US-04).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateLeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateLeadResponse>> Create(
        [FromBody] CreateLeadRequest request,
        CancellationToken cancellationToken)
        => Ok(await sender.Send(CreateLeadCommand.FromRequest(request), cancellationToken));
}
