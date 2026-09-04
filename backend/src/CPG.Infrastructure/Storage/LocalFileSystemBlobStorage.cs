using CPG.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace CPG.Infrastructure.Storage;

/// <summary>
/// Filesystem-backed <see cref="IBlobStorage"/> for local development / tests without any
/// storage emulator. Selected when <c>BlobStorage:Provider</c> is <c>Local</c>.
/// </summary>
public sealed class LocalFileSystemBlobStorage(IOptions<BlobStorageOptions> options) : IBlobStorage
{
    private readonly BlobStorageOptions _options = options.Value;

    public async Task<BlobUploadResult> UploadAsync(
        string containerName,
        string blobName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        _ = contentType;
        var path = ResolvePath(containerName, blobName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken).ConfigureAwait(false);

        var uri = new Uri($"{_options.LocalPublicBaseUri.TrimEnd('/')}/{containerName}/{blobName}");
        return new BlobUploadResult(uri, containerName, blobName);
    }

    public Task<Stream> DownloadAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(containerName, blobName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Blob not found.", path);
        }

        Stream stream = File.OpenRead(path);
        return Task.FromResult(stream);
    }

    public Task<bool> DeleteAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(containerName, blobName);
        if (!File.Exists(path))
        {
            return Task.FromResult(false);
        }

        File.Delete(path);
        return Task.FromResult(true);
    }

    private string ResolvePath(string containerName, string blobName)
    {
        var root = Path.GetFullPath(_options.LocalRootPath);
        var combined = Path.GetFullPath(Path.Combine(root, containerName, blobName));
        if (!combined.StartsWith(root, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Resolved blob path escapes the storage root.");
        }

        return combined;
    }
}
