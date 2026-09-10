namespace CPG.Application.Features.Rates;

/// <summary>
/// Computes a specialized-freight rate breakdown (base + cold-chain + fuel surcharges).
/// Pure, deterministic and in-memory so the endpoint stays well under the 500&#160;ms
/// budget in SPEC.md US-02 without any external geocoding round trip.
/// </summary>
public interface IRateEngine
{
    /// <summary>
    /// <paramref name="laneStatistic"/> is an optional, already-fetched historical lane
    /// statistic (see <see cref="Engine.HistoricalMarginAdjustmentHandler"/>); pass
    /// <see langword="null"/> when none exists for the lane/service/month.
    /// </summary>
    RateCalculationResponse Calculate(
        RateCalculationRequest request,
        Domain.Entities.LaneRateStatistic? laneStatistic = null);
}
