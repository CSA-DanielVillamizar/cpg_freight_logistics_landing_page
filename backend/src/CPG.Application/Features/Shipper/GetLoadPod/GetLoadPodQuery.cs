using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Common;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Shipper.GetLoadPod;

/// <summary>
/// Streams the proof-of-delivery PDF for one of the authenticated shipper's loads. The load
/// must belong to the caller and be <c>Delivered</c> with a POD on file.
/// </summary>
public sealed record GetLoadPodQuery(Guid LoadId) : IRequest<LoadPodContent>;

public sealed class GetLoadPodQueryHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IBlobStorage blobStorage)
    : IRequestHandler<GetLoadPodQuery, LoadPodContent>
{
    /// <summary>Blob container holding proof-of-delivery documents.</summary>
    public const string ContainerName = "pod-documents";

    public async Task<LoadPodContent> Handle(GetLoadPodQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var load = await dbContext.Loads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.LoadId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Load '{request.LoadId}' was not found.");

        if (load.ShipperUserId != userId)
        {
            throw new ForbiddenAccessException("This load does not belong to the current shipper.");
        }

        if (load.Status != LoadStatus.Delivered || string.IsNullOrWhiteSpace(load.PodBlobUri))
        {
            throw new DomainException($"Load {load.Reference} has no proof of delivery on file yet.");
        }

        var blobName = ExtractBlobName(load.PodBlobUri);
        var content = await blobStorage
            .DownloadAsync(ContainerName, blobName, cancellationToken)
            .ConfigureAwait(false);

        return new LoadPodContent
        {
            FileName = $"POD-{load.Reference}.pdf",
            ContentType = "application/pdf",
            Content = content,
        };
    }

    private static string ExtractBlobName(string blobUri)
    {
        var marker = $"/{ContainerName}/";
        var path = new Uri(blobUri).AbsolutePath;
        var index = path.IndexOf(marker, StringComparison.Ordinal);
        return index < 0 ? path.TrimStart('/') : path[(index + marker.Length)..];
    }
}
