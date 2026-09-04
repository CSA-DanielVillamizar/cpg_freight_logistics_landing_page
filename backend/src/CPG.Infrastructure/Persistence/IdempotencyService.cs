using CPG.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CPG.Infrastructure.Persistence;

/// <summary>PostgreSQL-backed store for <c>Idempotency-Key</c> replay (SPEC.md section 2).</summary>
public sealed class IdempotencyService(ApplicationDbContext dbContext) : IIdempotencyService
{
    public Task<bool> ExistsAsync(string idempotencyKey, CancellationToken cancellationToken = default)
        => dbContext.IdempotencyRecords.AnyAsync(r => r.Key == idempotencyKey, cancellationToken);

    public async Task<string?> TryGetResponseAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var record = await dbContext.IdempotencyRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Key == idempotencyKey, cancellationToken)
            .ConfigureAwait(false);

        return record?.ResponseBody;
    }

    public async Task StoreAsync(
        string idempotencyKey,
        string requestPath,
        int statusCode,
        string responseBody,
        CancellationToken cancellationToken = default)
    {
        dbContext.IdempotencyRecords.Add(new IdempotencyRecord
        {
            Key = idempotencyKey,
            RequestPath = requestPath,
            StatusCode = statusCode,
            ResponseBody = responseBody,
        });

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
