using CPG.Api.Infrastructure;
using CPG.Application.Features.Telemetry;
using CPG.Application.Features.Telemetry.IngestWebhook;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>
/// Inbound ELD telemetry webhooks (T-SDD Epica 2A). Anonymous at the ASP.NET Core auth layer —
/// trust instead comes from <see cref="TelemetryWebhookSignatureMiddleware"/>, which runs before
/// this action and rejects any request with a missing/invalid HMAC signature.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/webhooks/telemetry")]
public sealed class TelemetryWebhookController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Receives one GPS reading from a Samsara/Motive/KeepTruckin webhook. The signature has
    /// already been verified by the time this action runs.
    /// </summary>
    [HttpPost("{provider}")]
    [RequireTelemetrySignature]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Ingest(
        TelemetryProvider provider,
        [FromBody] TelemetryWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        await sender.Send(IngestTelemetryWebhookCommand.FromPayload(provider, payload), cancellationToken)
            .ConfigureAwait(false);
        return Accepted();
    }
}
