using CPG.Application.Common.Interfaces;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Admin.GetCarriers;

/// <summary>
/// Carrier accounts and their compliance documents for the admin control tower. Optional
/// <paramref name="Status"/> filters to a single compliance state (e.g. Under Review).
/// </summary>
public sealed record GetCarriersQuery(ComplianceStatus? Status = null)
    : IRequest<IReadOnlyList<CarrierComplianceView>>;

public sealed class GetCarriersQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetCarriersQuery, IReadOnlyList<CarrierComplianceView>>
{
    public async Task<IReadOnlyList<CarrierComplianceView>> Handle(
        GetCarriersQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Carriers
            .AsNoTracking()
            .Include(carrier => carrier.ComplianceDocuments)
            .AsQueryable();

        if (request.Status is { } status)
        {
            query = query.Where(carrier => carrier.ComplianceStatus == status);
        }

        var carriers = await query
            .OrderBy(carrier => carrier.ComplianceStatus == ComplianceStatus.UnderReview ? 0 : 1)
            .ThenByDescending(carrier => carrier.LastModifiedAtUtc ?? carrier.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return carriers
            .Select(carrier => new CarrierComplianceView
            {
                Id = carrier.Id,
                CompanyName = carrier.CompanyName,
                DotNumber = carrier.DotNumber,
                McNumber = carrier.McNumber,
                Status = carrier.ComplianceStatus,
                SubmittedAtUtc = carrier.ComplianceDocuments.Count == 0
                    ? null
                    : carrier.ComplianceDocuments.Max(document => document.CreatedAtUtc),
                LastReviewedAtUtc = carrier.ComplianceStatus is ComplianceStatus.Verified or ComplianceStatus.Rejected
                    ? carrier.LastModifiedAtUtc
                    : null,
                Documents = carrier.ComplianceDocuments
                    .OrderByDescending(document => document.CreatedAtUtc)
                    .Select(document => new CarrierDocumentView
                    {
                        Id = document.Id,
                        DocumentType = document.DocumentType,
                        OriginalFileName = document.OriginalFileName,
                        ContentType = document.ContentType,
                        SizeBytes = document.SizeBytes,
                        Status = document.Status,
                        UploadedAtUtc = document.CreatedAtUtc,
                    })
                    .ToList(),
            })
            .ToList();
    }
}
