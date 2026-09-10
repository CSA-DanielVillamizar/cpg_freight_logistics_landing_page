using CPG.Domain.Common;
using CPG.Domain.Enums;

namespace CPG.Domain.Entities;

/// <summary>
/// A pre-aggregated read model of historical rate-per-mile statistics for a given lane,
/// service type and month, refreshed nightly by <c>RecomputeLaneRateStatisticsJob</c>
/// (T-SDD Epica 5). Never written to from the request path of
/// <c>POST /api/rates/calculate</c> so the existing &lt;500ms SLA (US-02) is preserved.
/// </summary>
public class LaneRateStatistic : Entity
{
    public required string OriginZip3 { get; set; }

    public required string DestinationZip3 { get; set; }

    public required ServiceType ServiceType { get; set; }

    /// <summary>Calendar month (1-12) used to capture seasonality.</summary>
    public required int Month { get; set; }

    public required decimal AvgRatePerMileUsd { get; set; }

    public required decimal P25RatePerMileUsd { get; set; }

    public required decimal P75RatePerMileUsd { get; set; }

    public required int SampleSize { get; set; }

    public required DateTimeOffset ComputedAtUtc { get; set; }
}
