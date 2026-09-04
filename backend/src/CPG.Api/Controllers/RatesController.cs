using CPG.Application.Features.Rates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Specialized-freight rate calculation (SPEC.md US-02).</summary>
[AllowAnonymous]
public sealed class RatesController : ApiControllerBase
{
    /// <summary>
    /// Calculate a rate breakdown (base + cold-chain + fuel). Must respond in &lt;500&#160;ms
    /// (SPEC.md US-02). Contract: <see cref="RateCalculationRequest"/> / <see cref="RateCalculationResponse"/>.
    /// </summary>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(RateCalculationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Calculate([FromBody] RateCalculationRequest request)
    {
        _ = request;
        return NotImplementedYet("US-02");
    }
}
