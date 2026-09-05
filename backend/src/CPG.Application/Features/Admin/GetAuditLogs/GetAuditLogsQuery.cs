using CPG.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Admin.GetAuditLogs;

/// <summary>Most recent transactional audit trail rows, newest first (SPEC.md US-01/US-03).</summary>
public sealed record GetAuditLogsQuery : IRequest<IReadOnlyList<AuditLogEntryResponse>>;

public sealed record AuditLogEntryResponse
{
    public required Guid Id { get; init; }

    public required string Action { get; init; }

    public required string EntityName { get; init; }

    public string? EntityId { get; init; }

    public string? UserId { get; init; }

    public required DateTimeOffset TimestampUtc { get; init; }
}

public sealed class GetAuditLogsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogEntryResponse>>
{
    /// <summary>Hard cap so the admin feed can never return an unbounded result set.</summary>
    private const int MaxRows = 100;

    public async Task<IReadOnlyList<AuditLogEntryResponse>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.AuditLogEntries
            .AsNoTracking()
            .OrderByDescending(entry => entry.TimestampUtc)
            .Take(MaxRows)
            .Select(entry => new AuditLogEntryResponse
            {
                Id = entry.Id,
                Action = entry.Action,
                EntityName = entry.EntityName,
                EntityId = entry.EntityId,
                UserId = entry.UserId,
                TimestampUtc = entry.TimestampUtc,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
