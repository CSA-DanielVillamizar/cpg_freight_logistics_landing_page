using CPG.Api.Infrastructure;
using CPG.Application.Features.Admin.GetAuditLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Administrative endpoints. Restricted to the <c>Admin</c> role (SPEC.md US-01).</summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin")]
public sealed class AdminController(ISender sender) : ApiControllerBase
{
    /// <summary>
    /// Audit log feed backed by the real <c>AuditLogEntries</c> table. A <c>Carrier</c> or
    /// <c>Shipper</c> calling this receives 403 with "Access denied" (SPEC.md US-01 scenario 2).
    /// </summary>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditLogEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<AuditLogEntryResponse>>> GetAuditLogs(
        CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAuditLogsQuery(), cancellationToken));
}
