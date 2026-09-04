namespace CPG.Infrastructure.Storage;

/// <summary>Binds the <c>BlobStorage</c> configuration section.</summary>
public sealed class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    /// <summary>
    /// <c>Azure</c> (connection string - Azurite in dev / Azure Blob in prod),
    /// <c>AzureManagedIdentity</c> (endpoint + managed identity, no secrets), or
    /// <c>Local</c> (filesystem).
    /// </summary>
    public string Provider { get; set; } = "Azure";

    /// <summary>Azure Storage connection string (used when <see cref="Provider"/> is <c>Azure</c>).</summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Blob service endpoint, e.g. <c>https://acct.blob.core.windows.net/</c>. Used when
    /// <see cref="Provider"/> is <c>AzureManagedIdentity</c>.
    /// </summary>
    public string? ServiceUri { get; set; }

    /// <summary>
    /// Optional user-assigned managed identity client id for <c>AzureManagedIdentity</c>.
    /// Falls back to the <c>AZURE_CLIENT_ID</c> environment variable / system identity.
    /// </summary>
    public string? ManagedIdentityClientId { get; set; }

    /// <summary>Root directory used when <see cref="Provider"/> is <c>Local</c>.</summary>
    public string LocalRootPath { get; set; } = "./.blob-store";

    /// <summary>Public base URI used to compose returned blob URIs for the local provider.</summary>
    public string LocalPublicBaseUri { get; set; } = "http://localhost:5080/blobs";
}
