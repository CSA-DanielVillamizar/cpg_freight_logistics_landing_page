using CPG.Domain.Enums;

namespace CPG.Application.Features.Rates.Engine;

/// <summary>
/// Strategy for the base linehaul rate of one service line. Each specialized freight type
/// (cold chain, heavy haul, flatbed, FDOT concrete) has its own per-mile and per-weight
/// economics (SPEC.md US-02).
/// </summary>
public interface IServiceRateStrategy
{
    ServiceType ServiceType { get; }

    /// <summary>Base linehaul rate in USD before any surcharges.</summary>
    decimal ComputeBaseRate(double roadMiles, int weightLbs);
}
