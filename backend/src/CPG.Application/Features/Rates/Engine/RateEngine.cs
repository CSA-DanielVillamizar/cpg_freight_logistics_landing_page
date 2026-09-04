using CPG.Application.Common.Interfaces;
using CPG.Domain.Common;
using CPG.Domain.Enums;

namespace CPG.Application.Features.Rates.Engine;

/// <summary>
/// Orchestrates the rate calculation: distance -> per-service base rate (Strategy) ->
/// surcharge chain (Chain of Responsibility) -> total. Fully in-memory (SPEC.md US-02).
/// </summary>
public sealed class RateEngine : IRateEngine
{
    private readonly Dictionary<ServiceType, IServiceRateStrategy> _strategies;
    private readonly IDistanceCalculator _distance;
    private readonly IDateTimeProvider _clock;

    public RateEngine(
        IEnumerable<IServiceRateStrategy> strategies,
        IDistanceCalculator distance,
        IDateTimeProvider clock)
    {
        _strategies = strategies.ToDictionary(s => s.ServiceType);
        _distance = distance;
        _clock = clock;
    }

    public RateCalculationResponse Calculate(RateCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_strategies.TryGetValue(request.ServiceType, out var strategy))
        {
            throw new DomainException($"No rate strategy is registered for service type '{request.ServiceType}'.");
        }

        var miles = _distance.RoadMilesBetween(request.OriginZip, request.DestinationZip);
        var baseRate = strategy.ComputeBaseRate(miles, request.WeightLbs);

        var context = new SurchargeContext(request, miles, baseRate);

        // Build the chain: cold-chain first, then fuel (fuel is a % of the base rate only).
        var coldChain = new ColdChainSurchargeHandler();
        coldChain.SetNext(new FuelSurchargeHandler());
        coldChain.Handle(context);

        var total = baseRate + context.ColdChainSurcharge + context.FuelSurcharge;

        return new RateCalculationResponse
        {
            BaseRate = baseRate,
            ColdChainSurcharge = context.ColdChainSurcharge,
            FuelSurcharge = context.FuelSurcharge,
            TotalEstimated = Math.Round(total, 2, MidpointRounding.AwayFromZero),
            Currency = "USD",
            CalculatedAt = _clock.UtcNow,
        };
    }
}
