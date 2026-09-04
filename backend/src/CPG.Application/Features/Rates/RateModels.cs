using CPG.Domain.Enums;

namespace CPG.Application.Features.Rates;

/// <summary>
/// Rate calculation request. Mirrors the OpenAPI contract in SPEC.md section 4
/// (<c>POST /api/rates/calculate</c>).
/// </summary>
public sealed record RateCalculationRequest
{
    public required ServiceType ServiceType { get; init; }

    public required string OriginZip { get; init; }

    public required string DestinationZip { get; init; }

    public required int WeightLbs { get; init; }

    /// <summary>Target temperature in Celsius; required for <see cref="ServiceType.ColdChain"/>.</summary>
    public decimal? TargetTemperatureCelsius { get; init; }
}

/// <summary>
/// Rate calculation response. Mirrors the OpenAPI contract in SPEC.md section 4.
/// The computation must complete in under 500&#160;ms (SPEC.md US-02).
/// </summary>
public sealed record RateCalculationResponse
{
    public required decimal BaseRate { get; init; }

    public required decimal ColdChainSurcharge { get; init; }

    public required decimal FuelSurcharge { get; init; }

    public required decimal TotalEstimated { get; init; }

    public string Currency { get; init; } = "USD";

    public required DateTimeOffset CalculatedAt { get; init; }
}
