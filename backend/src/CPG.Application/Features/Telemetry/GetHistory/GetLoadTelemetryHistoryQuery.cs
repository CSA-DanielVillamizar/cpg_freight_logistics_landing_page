using CPG.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Telemetry.GetHistory;

/// <summary>Paginated GPS trail for a load, most recent first (T-SDD Epica 2A route-replay UI).</summary>
public sealed record GetLoadTelemetryHistoryQuery(Guid LoadId, int Take = 200)
    : IRequest<IReadOnlyList<TelemetryLogEntryResponse>>;

public sealed class GetLoadTelemetryHistoryQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetLoadTelemetryHistoryQuery, IReadOnlyList<TelemetryLogEntryResponse>>
{
    private const int MaxRows = 500;

    public async Task<IReadOnlyList<TelemetryLogEntryResponse>> Handle(
        GetLoadTelemetryHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var take = Math.Clamp(request.Take, 1, MaxRows);

        return await dbContext.TelemetryLogs
            .AsNoTracking()
            .Where(log => log.LoadId == request.LoadId)
            .OrderByDescending(log => log.RecordedAtUtc)
            .Take(take)
            .Select(log => new TelemetryLogEntryResponse
            {
                Latitude = log.Latitude,
                Longitude = log.Longitude,
                SpeedMph = log.SpeedMph,
                HeadingDegrees = log.HeadingDegrees,
                RecordedAtUtc = log.RecordedAtUtc,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
