using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Admin.ReviewCarrier;

public sealed class ReviewCarrierComplianceCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<ReviewCarrierComplianceCommand, CarrierComplianceView>
{
    public async Task<CarrierComplianceView> Handle(
        ReviewCarrierComplianceCommand request,
        CancellationToken cancellationToken)
    {
        var adminUserId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var carrier = await dbContext.Carriers
            .Include(c => c.ComplianceDocuments)
            .FirstOrDefaultAsync(c => c.Id == request.CarrierId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Carrier '{request.CarrierId}' was not found.");

        var approved = request.Decision == ReviewDecision.Approve;

        // Domain guard: throws DomainException (-> 409) when there is nothing to review.
        var changedDocumentIds = carrier.CompleteComplianceReview(approved);

        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "CarrierComplianceReviewed",
            EntityName = nameof(Carrier),
            EntityId = carrier.Id.ToString(),
            UserId = adminUserId.ToString(),
            TimestampUtc = clock.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new
            {
                decision = request.Decision.ToString(),
                newStatus = carrier.ComplianceStatus.ToString(),
                changedDocumentIds,
                notes = request.Notes,
            }),
        });

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CarrierComplianceView
        {
            Id = carrier.Id,
            CompanyName = carrier.CompanyName,
            DotNumber = carrier.DotNumber,
            McNumber = carrier.McNumber,
            Status = carrier.ComplianceStatus,
            SubmittedAtUtc = carrier.ComplianceDocuments.Count == 0
                ? null
                : carrier.ComplianceDocuments.Max(document => document.CreatedAtUtc),
            LastReviewedAtUtc = clock.UtcNow,
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
        };
    }
}
