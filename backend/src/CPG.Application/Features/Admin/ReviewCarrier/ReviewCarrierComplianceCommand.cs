using MediatR;

namespace CPG.Application.Features.Admin.ReviewCarrier;

/// <summary>
/// An administrator approves or rejects a carrier's compliance. Runs in one EF Core
/// transaction: the carrier (and its under-review documents) move to
/// <c>Verified</c>/<c>Rejected</c> and an <c>AuditLogEntry</c> is written.
/// </summary>
public sealed record ReviewCarrierComplianceCommand(
    Guid CarrierId,
    ReviewDecision Decision,
    string? Notes) : IRequest<CarrierComplianceView>;
