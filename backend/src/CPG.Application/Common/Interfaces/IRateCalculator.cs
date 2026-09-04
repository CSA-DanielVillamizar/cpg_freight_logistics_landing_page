using CPG.Application.Features.Rates;

namespace CPG.Application.Common.Interfaces;

/// <summary>
/// Computes a specialized-freight rate breakdown (base + cold-chain + fuel surcharges).
/// Implemented in Infrastructure against an in-memory lane/rate matrix to satisfy the
/// &lt;500&#160;ms budget in SPEC.md US-02 without an external geocoding round trip.
/// </summary>
public interface IRateCalculator
{
    RateCalculationResponse Calculate(RateCalculationRequest request);
}
