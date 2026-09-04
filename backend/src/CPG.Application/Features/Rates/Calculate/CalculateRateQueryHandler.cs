using MediatR;

namespace CPG.Application.Features.Rates.Calculate;

/// <summary>
/// Thin handler: delegates to the pure in-memory <see cref="IRateEngine"/>. No DB or
/// external calls so the request stays far under the 500&#160;ms budget (SPEC.md US-02).
/// </summary>
public sealed class CalculateRateQueryHandler(IRateEngine rateEngine)
    : IRequestHandler<CalculateRateQuery, RateCalculationResponse>
{
    public Task<RateCalculationResponse> Handle(CalculateRateQuery request, CancellationToken cancellationToken)
        => Task.FromResult(rateEngine.Calculate(request.ToRequest()));
}
