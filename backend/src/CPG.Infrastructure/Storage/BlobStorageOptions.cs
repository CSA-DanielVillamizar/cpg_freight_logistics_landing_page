namespace CPG.Infrastructure.Storage;

/// <summary>Binds the <c>BlobStorage</c> configuration section.</summary>
public sealed class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    /// <summary><c>Azure</c> (Azurite in dev / Azure Blob in prod) or <c>Local</c> filesystem.</summary>
    public string Provider { get; set; } = "Azure";

    /// <summary>Azure Storage connection string (Azurite default in dev).</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Root directory used when <see cref="Provider"/> is <c>Local</c>.</summary>
    public string LocalRootPath { get; set; } = "./.blob-store";

    /// <summary>Public base URI used to compose returned blob URIs for the local provider.</summary>
    public string LocalPublicBaseUri { get; set; } = "http://localhost:5080/blobs";
}
