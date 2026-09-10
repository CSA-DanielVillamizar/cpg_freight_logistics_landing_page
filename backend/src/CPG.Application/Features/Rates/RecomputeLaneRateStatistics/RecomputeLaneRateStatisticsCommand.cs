using MediatR;

namespace CPG.Application.Features.Rates.RecomputeLaneRateStatistics;

/// <summary>
/// Rebuilds the <c>LaneRateStatistics</c> read model from the last 18 months of delivered
/// loads (T-SDD Epica 5). Intended to run nightly, off the request path.
/// </summary>
public sealed record RecomputeLaneRateStatisticsCommand : IRequest<RecomputeLaneRateStatisticsResponse>;

public sealed record RecomputeLaneRateStatisticsResponse(int GroupsUpserted, DateTimeOffset ComputedAtUtc);
