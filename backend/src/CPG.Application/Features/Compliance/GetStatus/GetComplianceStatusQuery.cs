using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Compliance.GetStatus;

/// <summary>Returns the authenticated carrier's compliance snapshot (SPEC.md US-03 portal).</summary>
public sealed record GetComplianceStatusQuery : IRequest<ComplianceStatusResponse>;

public sealed class GetComplianceStatusQueryHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser)
    : IRequestHandler<GetComplianceStatusQuery, ComplianceStatusResponse>
{
    public async Task<ComplianceStatusResponse> Handle(
        GetComplianceStatusQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var carrier = await dbContext.Carriers
            .AsNoTracking()
            .Include(c => c.ComplianceDocuments)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("No carrier account is linked to the current user.");

        return new ComplianceStatusResponse
        {
            CarrierId = carrier.Id,
            CompanyName = carrier.CompanyName,
            Status = carrier.ComplianceStatus,
            Documents = carrier.ComplianceDocuments
                .OrderByDescending(d => d.CreatedAtUtc)
                .Select(d => new ComplianceDocumentSummary
                {
                    Id = d.Id,
                    DocumentType = d.DocumentType,
                    OriginalFileName = d.OriginalFileName,
                    SizeBytes = d.SizeBytes,
                    Status = d.Status,
                    UploadedAtUtc = d.CreatedAtUtc,
                })
                .ToList(),
        };
    }
}
