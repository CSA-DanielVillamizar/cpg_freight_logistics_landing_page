using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Application.Features.Compliance.Upload;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Admin.GetCarrierDocument;

/// <summary>Streams a carrier compliance document's bytes back to an authorised admin.</summary>
public sealed record GetCarrierDocumentQuery(Guid CarrierId, Guid DocumentId)
    : IRequest<CarrierDocumentContent>;

public sealed class GetCarrierDocumentQueryHandler(
    IApplicationDbContext dbContext,
    IBlobStorage blobStorage)
    : IRequestHandler<GetCarrierDocumentQuery, CarrierDocumentContent>
{
    public async Task<CarrierDocumentContent> Handle(
        GetCarrierDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var document = await dbContext.ComplianceDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                d => d.Id == request.DocumentId && d.CarrierId == request.CarrierId,
                cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Compliance document was not found for this carrier.");

        var blobName = ExtractBlobName(document.BlobUri);
        var content = await blobStorage
            .DownloadAsync(UploadComplianceDocumentCommandHandler.ContainerName, blobName, cancellationToken)
            .ConfigureAwait(false);

        return new CarrierDocumentContent
        {
            FileName = document.OriginalFileName,
            ContentType = document.ContentType,
            Content = content,
        };
    }

    /// <summary>Blob key = everything after "/&lt;container&gt;/" in the stored absolute URI.</summary>
    private static string ExtractBlobName(string blobUri)
    {
        var marker = $"/{UploadComplianceDocumentCommandHandler.ContainerName}/";
        var path = new Uri(blobUri).AbsolutePath;
        var index = path.IndexOf(marker, StringComparison.Ordinal);
        return index < 0 ? path.TrimStart('/') : path[(index + marker.Length)..];
    }
}
