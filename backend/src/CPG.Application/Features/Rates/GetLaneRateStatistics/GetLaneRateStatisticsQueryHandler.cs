using CPG.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace CPG.Application.Features.Rates.GetLaneRateStatistics;

public sealed class GetLaneRateStatisticsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetLaneRateStatisticsQuery, IReadOnlyList<LaneRateStatisticResponse>>
{
    public async Task<IReadOnlyList<LaneRateStatisticResponse>> Handle(
        GetLaneRateStatisticsQuery request,
        CancellationToken cancellationToken)
        => await dbContext.LaneRateStatistics
            .AsNoTracking()
            .OrderByDescending(s => s.ComputedAtUtc)
            .Select(s => new LaneRateStatisticResponse(
                s.Id,
                s.OriginZip3,
                s.DestinationZip3,
                s.ServiceType,
                s.Month,
                s.AvgRatePerMileUsd,
                s.P25RatePerMileUsd,
                s.P75RatePerMileUsd,
                s.SampleSize,
                s.ComputedAtUtc))
            .ToListAsync(cancellationToken);
}
