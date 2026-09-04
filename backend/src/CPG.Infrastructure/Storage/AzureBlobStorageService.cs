using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CPG.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace CPG.Infrastructure.Storage;

/// <summary>
/// <see cref="IBlobStorage"/> over Azure Blob Storage. In development this points at Azurite
/// (see docker-compose); in production at an Azure Storage account (SPEC.md US-03).
/// </summary>
public sealed class AzureBlobStorageService : IBlobStorage
{
    private readonly BlobServiceClient _client;

    public AzureBlobStorageService(IOptions<BlobStorageOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var connectionString = options.Value.ConnectionString
            ?? throw new InvalidOperationException("BlobStorage:ConnectionString is required for the Azure provider.");
        _client = new BlobServiceClient(connectionString);
    }

    public async Task<BlobUploadResult> UploadAsync(
        string containerName,
        string blobName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(
            content,
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } },
            cancellationToken).ConfigureAwait(false);

        return new BlobUploadResult(blob.Uri, containerName, blobName);
    }

    public async Task<Stream> DownloadAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default)
    {
        var blob = _client.GetBlobContainerClient(containerName).GetBlobClient(blobName);
        var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.Value.Content;
    }

    public async Task<bool> DeleteAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default)
    {
        var blob = _client.GetBlobContainerClient(containerName).GetBlobClient(blobName);
        var response = await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.Value;
    }
}
