using CPG.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace CPG.Application.Features.Rates.Calculate;

/// <summary>
/// Delegates to the pure in-memory <see cref="IRateEngine"/>, first fetching the (if any)
/// historical <see cref="CPG.Domain.Entities.LaneRateStatistic"/> for this lane/service/month
/// via a single indexed read (T-SDD Epica 5). Still far under the 500&#160;ms budget
/// (SPEC.md US-02) — one indexed row lookup, no aggregation on the request path.
/// </summary>
public sealed class CalculateRateQueryHandler(IRateEngine rateEngine, IApplicationDbContext dbContext, IDateTimeProvider clock)
    : IRequestHandler<CalculateRateQuery, RateCalculationResponse>
{
    public async Task<RateCalculationResponse> Handle(CalculateRateQuery request, CancellationToken cancellationToken)
    {
        var originZip3 = request.OriginZip.Length >= 3 ? request.OriginZip[..3] : request.OriginZip;
        var destinationZip3 = request.DestinationZip.Length >= 3 ? request.DestinationZip[..3] : request.DestinationZip;
        var month = clock.UtcNow.Month;

        var statistic = await dbContext.LaneRateStatistics
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.OriginZip3 == originZip3
                    && s.DestinationZip3 == destinationZip3
                    && s.ServiceType == request.ServiceType
                    && s.Month == month,
                cancellationToken);

        return rateEngine.Calculate(request.ToRequest(), statistic);
    }
}
