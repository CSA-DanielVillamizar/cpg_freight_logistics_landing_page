using CPG.Domain.Enums;

namespace CPG.Application.Features.Rates.Engine;

public abstract class ServiceRateStrategyBase : IServiceRateStrategy
{
    public abstract ServiceType ServiceType { get; }

    protected abstract decimal PerMileRate { get; }

    protected abstract decimal PerLbRate { get; }

    protected abstract decimal MinimumRate { get; }

    public decimal ComputeBaseRate(double roadMiles, int weightLbs)
    {
        var miles = (decimal)Math.Max(0d, roadMiles);
        var raw = (miles * PerMileRate) + (weightLbs * PerLbRate);
        return Math.Round(Math.Max(MinimumRate, raw), 2, MidpointRounding.AwayFromZero);
    }
}

/// <summary>Temperature-controlled reefer freight.</summary>
public sealed class ColdChainRateStrategy : ServiceRateStrategyBase
{
    public override ServiceType ServiceType => ServiceType.ColdChain;

    protected override decimal PerMileRate => 4.00m;

    protected override decimal PerLbRate => 0.008m;

    protected override decimal MinimumRate => 650m;
}

/// <summary>Over-dimensional / superload multi-axle transport.</summary>
public sealed class HeavyHaulRateStrategy : ServiceRateStrategyBase
{
    public override ServiceType ServiceType => ServiceType.HeavyHaul;

    protected override decimal PerMileRate => 6.25m;

    protected override decimal PerLbRate => 0.011m;

    protected override decimal MinimumRate => 900m;
}

/// <summary>Standard 48'/53' flatbed and step-deck freight.</summary>
public sealed class FlatbedRateStrategy : ServiceRateStrategyBase
{
    public override ServiceType ServiceType => ServiceType.Flatbed;

    protected override decimal PerMileRate => 3.35m;

    protected override decimal PerLbRate => 0.004m;

    protected override decimal MinimumRate => 500m;
}

/// <summary>FDOT concrete barricade delivery and crane staging.</summary>
public sealed class FdotConcreteRateStrategy : ServiceRateStrategyBase
{
    public override ServiceType ServiceType => ServiceType.FdotConcrete;

    protected override decimal PerMileRate => 5.00m;

    protected override decimal PerLbRate => 0.006m;

    protected override decimal MinimumRate => 750m;
}
