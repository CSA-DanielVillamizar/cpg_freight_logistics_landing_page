namespace CPG.Application.Features.Rates;

/// <summary>
/// Confidence signal for <see cref="RateCalculationResponse.SuggestedRateUsd"/>, driven by how
/// much historical data backs the suggestion (T-SDD Epica 5).
/// </summary>
public enum RateConfidence
{
    /// <summary>Fewer than 10 historical samples, or none at all (safe fallback).</summary>
    Low = 1,

    /// <summary>10-29 historical samples for this lane/service/month.</summary>
    Medium = 2,

    /// <summary>30 or more historical samples for this lane/service/month.</summary>
    High = 3,
}
