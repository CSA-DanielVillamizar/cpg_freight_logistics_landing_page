using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Niche landing-page lead capture (SPEC.md US-04).</summary>
[AllowAnonymous]
public sealed class LeadsController : ApiControllerBase
{
    /// <summary>
    /// Submit an enterprise inquiry. On success the lead is persisted with status <c>New</c>
    /// and a <c>LeadCreated</c> event is published to RabbitMQ (SPEC.md US-04).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create() => NotImplementedYet("US-04");
}
