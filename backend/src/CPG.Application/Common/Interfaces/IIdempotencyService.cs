namespace CPG.Application.Common.Interfaces;

/// <summary>
/// Persists <c>Idempotency-Key</c> results so a retried request (e.g. a carrier losing
/// cellular signal mid-POST on a highway) returns the original outcome instead of creating
/// a duplicate load (SPEC.md section 2).
/// </summary>
public interface IIdempotencyService
{
    Task<bool> ExistsAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    Task<string?> TryGetResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    Task StoreAsync(
        string idempotencyKey,
        string requestPath,
        int statusCode,
        string responseBody,
        CancellationToken cancellationToken = default);
}
