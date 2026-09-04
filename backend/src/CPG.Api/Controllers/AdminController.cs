using CPG.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Administrative endpoints. Restricted to the <c>Admin</c> role (SPEC.md US-01).</summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin")]
public sealed class AdminController : ApiControllerBase
{
    /// <summary>
    /// Audit log feed. A <c>Carrier</c> or <c>Shipper</c> calling this receives 403 with
    /// "Access denied" (SPEC.md US-01 scenario 2).
    /// </summary>
    [HttpGet("audit-logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetAuditLogs() => Ok(Array.Empty<object>());
}
