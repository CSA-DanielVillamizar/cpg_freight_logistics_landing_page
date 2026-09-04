namespace CPG.Application.Common.Interfaces;

/// <summary>Result of persisting a blob.</summary>
/// <param name="Uri">Absolute URI of the stored object.</param>
/// <param name="ContainerName">Logical container the blob was written to.</param>
/// <param name="BlobName">Generated blob key within the container.</param>
public readonly record struct BlobUploadResult(Uri Uri, string ContainerName, string BlobName);

/// <summary>
/// Abstraction over object storage for compliance documents (SPEC.md US-03).
/// Dev: Azurite (emulated Azure Blob). Prod: Azure Blob Storage.
/// </summary>
public interface IBlobStorage
{
    Task<BlobUploadResult> UploadAsync(
        string containerName,
        string blobName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default);
}
