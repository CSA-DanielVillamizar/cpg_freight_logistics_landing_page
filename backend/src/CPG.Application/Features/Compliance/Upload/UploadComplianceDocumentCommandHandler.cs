using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Compliance.Upload;

public sealed class UploadComplianceDocumentCommandHandler(
    IApplicationDbContext dbContext,
    IBlobStorage blobStorage,
    ICurrentUser currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<UploadComplianceDocumentCommand, UploadComplianceDocumentResult>
{
    public const string ContainerName = "compliance-documents";

    public async Task<UploadComplianceDocumentResult> Handle(
        UploadComplianceDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var carrier = await dbContext.Carriers
            .Include(c => c.ComplianceDocuments)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("No carrier account is linked to the current user.");

        // 1. Persist the blob first. If the DB transaction fails we clean it up.
        var extension = Path.GetExtension(request.FileName);
        var blobName = $"{carrier.Id}/{Guid.NewGuid():N}{extension}";
        var upload = await blobStorage
            .UploadAsync(ContainerName, blobName, request.Content, request.ContentType, cancellationToken)
            .ConfigureAwait(false);

        try
        {
            // 2. Single transaction: add document + move carrier to Under Review + audit row.
            var document = carrier.SubmitComplianceDocument(
                request.DocumentType,
                upload.Uri.ToString(),
                request.FileName,
                request.ContentType,
                request.SizeBytes,
                clock.UtcNow);

            // The document has a client-assigned Guid key, so EF would otherwise treat it as
            // an existing row when discovered via the navigation. Force the INSERT explicitly.
            dbContext.ComplianceDocuments.Add(document);

            dbContext.AuditLogEntries.Add(new AuditLogEntry
            {
                Action = "ComplianceDocumentUploaded",
                EntityName = nameof(Carrier),
                EntityId = carrier.Id.ToString(),
                UserId = userId.ToString(),
                TimestampUtc = clock.UtcNow,
                TraceId = Activity.Current?.TraceId.ToString(),
                DataJson = JsonSerializer.Serialize(new
                {
                    documentId = document.Id,
                    documentType = request.DocumentType.ToString(),
                    request.FileName,
                    request.ContentType,
                    request.SizeBytes,
                    blobName = upload.BlobName,
                }),
            });

            // SaveChanges commits the transaction; the domain event is dispatched to
            // RabbitMQ afterwards by DispatchDomainEventsInterceptor.
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new UploadComplianceDocumentResult
            {
                CarrierId = carrier.Id,
                DocumentId = document.Id,
                Status = carrier.ComplianceStatus,
                BlobUri = upload.Uri.ToString(),
            };
        }
        catch
        {
            await blobStorage.DeleteAsync(ContainerName, blobName, CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }
}
