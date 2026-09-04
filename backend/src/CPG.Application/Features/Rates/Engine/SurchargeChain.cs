namespace CPG.Application.Features.Rates.Engine;

/// <summary>Mutable accumulator threaded through the surcharge chain.</summary>
public sealed class SurchargeContext(RateCalculationRequest request, double roadMiles, decimal baseRate)
{
    public RateCalculationRequest Request { get; } = request;

    public double RoadMiles { get; } = roadMiles;

    public decimal BaseRate { get; } = baseRate;

    public decimal ColdChainSurcharge { get; set; }

    public decimal FuelSurcharge { get; set; }
}

/// <summary>
/// Chain of Responsibility link. Each handler applies its surcharge (if applicable) and
/// forwards to the next (SPEC.md US-02 - dynamic surcharges by cargo type).
/// </summary>
public abstract class SurchargeHandler
{
    private SurchargeHandler? _next;

    public SurchargeHandler SetNext(SurchargeHandler next)
    {
        _next = next;
        return next;
    }

    public void Handle(SurchargeContext context)
    {
        Apply(context);
        _next?.Handle(context);
    }

    protected abstract void Apply(SurchargeContext context);

    protected static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

/// <summary>
/// Cold-chain surcharge: scales with cargo weight and how far the target temperature is
/// below freezing. Applied only to <see cref="Domain.Enums.ServiceType.ColdChain"/>.
/// </summary>
public sealed class ColdChainSurchargeHandler : SurchargeHandler
{
    private const decimal PerLbPerDegree = 0.0005m;

    protected override void Apply(SurchargeContext context)
    {
        if (context.Request.ServiceType != Domain.Enums.ServiceType.ColdChain)
        {
            return;
        }

        var target = context.Request.TargetTemperatureCelsius ?? 0m;
        var degreesBelowFreezing = Math.Max(0m, -target);
        context.ColdChainSurcharge = Round(degreesBelowFreezing * context.Request.WeightLbs * PerLbPerDegree);
    }
}

/// <summary>Fuel surcharge: a flat percentage of the base linehaul rate. Always applied.</summary>
public sealed class FuelSurchargeHandler : SurchargeHandler
{
    private const decimal FuelSurchargeRate = 0.15m;

    protected override void Apply(SurchargeContext context)
        => context.FuelSurcharge = Round(context.BaseRate * FuelSurchargeRate);
}
