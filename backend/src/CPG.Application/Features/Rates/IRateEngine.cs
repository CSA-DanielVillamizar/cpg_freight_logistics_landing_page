namespace CPG.Application.Features.Rates;

/// <summary>
/// Computes a specialized-freight rate breakdown (base + cold-chain + fuel surcharges).
/// Pure, deterministic and in-memory so the endpoint stays well under the 500&#160;ms
/// budget in SPEC.md US-02 without any external geocoding round trip.
/// </summary>
public interface IRateEngine
{
    RateCalculationResponse Calculate(RateCalculationRequest request);
}
