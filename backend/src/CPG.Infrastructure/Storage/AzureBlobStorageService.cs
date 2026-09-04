using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CPG.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace CPG.Infrastructure.Storage;

/// <summary>
/// <see cref="IBlobStorage"/> over Azure Blob Storage. Development points at Azurite via a
/// connection string (<c>Provider = Azure</c>); production uses the account endpoint plus a
/// managed identity with no secrets (<c>Provider = AzureManagedIdentity</c>) - SPEC.md US-03.
/// </summary>
public sealed class AzureBlobStorageService : IBlobStorage
{
    private readonly BlobServiceClient _client;

    public AzureBlobStorageService(IOptions<BlobStorageOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var config = options.Value;

        if (string.Equals(config.Provider, "AzureManagedIdentity", StringComparison.OrdinalIgnoreCase))
        {
            var serviceUri = config.ServiceUri
                ?? throw new InvalidOperationException(
                    "BlobStorage:ServiceUri is required for the AzureManagedIdentity provider.");

            var credential = string.IsNullOrWhiteSpace(config.ManagedIdentityClientId)
                ? new DefaultAzureCredential()
                : new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = config.ManagedIdentityClientId,
                });

            _client = new BlobServiceClient(new Uri(serviceUri), credential);
        }
        else
        {
            var connectionString = config.ConnectionString
                ?? throw new InvalidOperationException(
                    "BlobStorage:ConnectionString is required for the Azure provider.");
            _client = new BlobServiceClient(connectionString);
        }
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
