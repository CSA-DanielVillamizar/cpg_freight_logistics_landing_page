using System.Diagnostics;
using CPG.Application.Features.Rates;
using CPG.Application.Features.Rates.Calculate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Specialized-freight rate calculation (SPEC.md US-02).</summary>
[AllowAnonymous]
public sealed class RatesController(ISender sender) : ApiControllerBase
{
    /// <summary>
    /// Calculate a rate breakdown (base + cold-chain + fuel). Responds in &lt;500&#160;ms
    /// (SPEC.md US-02). The <c>Server-Timing</c> / <c>X-Rate-Compute-Ms</c> headers report the
    /// server-side computation time.
    /// </summary>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(RateCalculationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RateCalculationResponse>> Calculate(
        [FromBody] RateCalculationRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await sender.Send(CalculateRateQuery.FromRequest(request), cancellationToken);
        stopwatch.Stop();

        var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
        Response.Headers["X-Rate-Compute-Ms"] = elapsedMs.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        Response.Headers["Server-Timing"] = $"rate;dur={elapsedMs:F2}";

        return Ok(response);
    }
}
