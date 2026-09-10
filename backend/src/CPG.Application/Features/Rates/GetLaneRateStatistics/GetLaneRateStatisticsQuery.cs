using CPG.Domain.Enums;
using MediatR;

namespace CPG.Application.Features.Rates.GetLaneRateStatistics;

/// <summary>Admin-only read of the pre-aggregated historical lane rate table (T-SDD Epica 5).</summary>
public sealed record GetLaneRateStatisticsQuery : IRequest<IReadOnlyList<LaneRateStatisticResponse>>;

public sealed record LaneRateStatisticResponse(
    Guid Id,
    string OriginZip3,
    string DestinationZip3,
    ServiceType ServiceType,
    int Month,
    decimal AvgRatePerMileUsd,
    decimal P25RatePerMileUsd,
    decimal P75RatePerMileUsd,
    int SampleSize,
    DateTimeOffset ComputedAtUtc);
