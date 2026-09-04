namespace CPG.Infrastructure.Persistence;

/// <summary>
/// Stored outcome of a request that carried an <c>Idempotency-Key</c>. A retry with the
/// same key replays this response instead of re-executing the operation (SPEC.md section 2).
/// </summary>
public class IdempotencyRecord
{
    public required string Key { get; set; }

    public required string RequestPath { get; set; }

    public required int StatusCode { get; set; }

    public required string ResponseBody { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
