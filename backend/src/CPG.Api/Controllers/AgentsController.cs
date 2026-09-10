using CPG.Api.Infrastructure;
using CPG.Application.Features.Agents;
using CPG.Application.Features.Agents.AcceptInvitation;
using CPG.Application.Features.Agents.GetClients;
using CPG.Application.Features.Agents.GetDashboard;
using CPG.Application.Features.Agents.InviteClient;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Independent Agent workspace: client invitations and the commission dashboard (T-SDD Epica 4).</summary>
[Authorize]
public sealed class AgentsController(ISender sender) : ApiControllerBase
{
    /// <summary>Invites a prospective client to join CPG under the authenticated Agent's roster.</summary>
    [Authorize(Policy = AuthorizationPolicies.AgentOnly)]
    [HttpPost("clients/invite")]
    [ProducesResponseType(typeof(AgentClientInvitationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AgentClientInvitationResponse>> InviteClient(
        [FromBody] InviteClientRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(new InviteClientCommand(request.InvitedEmail), cancellationToken);
        return Created($"/api/agents/clients/{response.InvitationId}", response);
    }

    /// <summary>The authenticated Agent's client invitations, newest first.</summary>
    [Authorize(Policy = AuthorizationPolicies.AgentOnly)]
    [HttpGet("clients")]
    [ProducesResponseType(typeof(IReadOnlyList<AgentClientView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AgentClientView>>> GetClients(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAgentClientsQuery(), cancellationToken));

    /// <summary>
    /// The Agent's commission dashboard. 403 if <paramref name="id"/> is not the caller's own
    /// agent profile — an Agent can never read another Agent's numbers.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.AgentOnly)]
    [HttpGet("{id:guid}/dashboard")]
    [ProducesResponseType(typeof(AgentDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgentDashboardResponse>> GetDashboard(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAgentDashboardQuery(id), cancellationToken));

    /// <summary>
    /// The authenticated invitee accepts an Agent's client invitation. Any authenticated role
    /// may call this (not Agent-restricted) — the invitee is typically a Shipper.
    /// </summary>
    [HttpPost("invitations/{token}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AcceptInvitation(string token, CancellationToken cancellationToken)
    {
        await sender.Send(new AcceptClientInvitationCommand(token), cancellationToken).ConfigureAwait(false);
        return Ok();
    }
}
