using CPG.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Carrier compliance document upload and review (SPEC.md US-03).</summary>
[Authorize(Policy = AuthorizationPolicies.CarrierOnly)]
public sealed class ComplianceController : ApiControllerBase
{
    /// <summary>
    /// Upload a legal document (COI, insurance, FDOT permit). Stored in blob storage; the
    /// carrier record transitions to <c>Under Review</c> and an audit entry is written
    /// (SPEC.md US-03).
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Upload(IFormFile file)
    {
        _ = file;
        return NotImplementedYet("US-03");
    }
}
