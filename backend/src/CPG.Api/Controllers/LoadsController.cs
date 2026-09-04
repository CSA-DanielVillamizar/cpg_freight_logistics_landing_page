using CPG.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Freight load board (SPEC.md section 2 - idempotency + optimistic concurrency).</summary>
[Authorize]
public sealed class LoadsController : ApiControllerBase
{
    /// <summary>
    /// Create a load. Requires an <c>Idempotency-Key: &lt;UUID&gt;</c> header so a retry after a
    /// dropped cellular connection does not create a duplicate (SPEC.md section 2).
    /// </summary>
    [HttpPost]
    [RequireIdempotencyKey]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Create() => NotImplementedYet("the load board slice");

    /// <summary>List loads on the board.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult List() => Ok(Array.Empty<object>());
}
