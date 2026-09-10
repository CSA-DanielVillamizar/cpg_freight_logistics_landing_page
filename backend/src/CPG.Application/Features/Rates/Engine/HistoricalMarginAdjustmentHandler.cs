namespace CPG.Application.Features.Rates.Engine;

/// <summary>
/// Appends a market-informed rate suggestion to the end of the surcharge chain
/// (Base -> Surcharges -> HistoricalMarginAdjustment, T-SDD Epica 5). The historical lookup
/// itself is an O(1) read on the unique (OriginZip3, DestinationZip3, ServiceType, Month) index
/// and happens once in <c>CalculateRateQueryHandler</c> before the chain runs — this handler is
/// synchronous like every other link, so it never talks to the database directly.
/// </summary>
public sealed class HistoricalMarginAdjustmentHandler : SurchargeHandler
{
    private const int HighConfidenceSampleSize = 30;
    private const int MediumConfidenceSampleSize = 10;

    protected override void Apply(SurchargeContext context)
    {
        var statistic = context.LaneStatistic;
        var baseTotal = context.BaseRate + context.ColdChainSurcharge + context.FuelSurcharge;

        if (statistic is null || statistic.SampleSize <= 0)
        {
            context.SuggestedRateUsd = Round(baseTotal);
            context.SuggestedRateConfidence = RateConfidence.Low;
            return;
        }

        context.SuggestedRateUsd = Round(statistic.AvgRatePerMileUsd * (decimal)context.RoadMiles);
        context.SuggestedRateConfidence = statistic.SampleSize switch
        {
            >= HighConfidenceSampleSize => RateConfidence.High,
            >= MediumConfidenceSampleSize => RateConfidence.Medium,
            _ => RateConfidence.Low,
        };
    }
}
