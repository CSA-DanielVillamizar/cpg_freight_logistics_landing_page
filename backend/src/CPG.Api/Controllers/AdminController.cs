using CPG.Api.Infrastructure;
using CPG.Application.Features.Admin;
using CPG.Application.Features.Admin.GetAuditLogs;
using CPG.Application.Features.Admin.GetCarrierDocument;
using CPG.Application.Features.Admin.GetCarriers;
using CPG.Application.Features.Admin.ReviewCarrier;
using CPG.Domain.Enums;
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

    /// <summary>
    /// Carrier accounts and their compliance documents for the control tower. Optional
    /// <c>status</c> query filters to a single compliance state (e.g. <c>UnderReview</c>).
    /// </summary>
    [HttpGet("carriers")]
    [ProducesResponseType(typeof(IReadOnlyList<CarrierComplianceView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<CarrierComplianceView>>> GetCarriers(
        [FromQuery] ComplianceStatus? status,
        CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetCarriersQuery(status), cancellationToken));

    /// <summary>
    /// Approve or reject a carrier's compliance. Approval moves the carrier to
    /// <c>Verified</c>; rejection to <c>Rejected</c>. Writes an audit row. Returns 409 if the
    /// carrier has no documents to review, 404 if the carrier does not exist.
    /// </summary>
    [HttpPost("carriers/{id:guid}/review")]
    [ProducesResponseType(typeof(CarrierComplianceView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CarrierComplianceView>> ReviewCarrier(
        Guid id,
        [FromBody] ReviewCarrierRequest request,
        CancellationToken cancellationToken)
        => Ok(await sender.Send(
            new ReviewCarrierComplianceCommand(id, request.Decision, request.Notes),
            cancellationToken));

    /// <summary>Streams the bytes of one of a carrier's compliance documents (COI, insurance, permit).</summary>
    [HttpGet("carriers/{carrierId:guid}/documents/{documentId:guid}/content")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCarrierDocument(
        Guid carrierId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var document = await sender.Send(
            new GetCarrierDocumentQuery(carrierId, documentId),
            cancellationToken);

        return File(document.Content, document.ContentType, document.FileName);
    }
}

/// <summary>Body for <c>POST /api/admin/carriers/{id}/review</c>.</summary>
public sealed record ReviewCarrierRequest(ReviewDecision Decision, string? Notes);
