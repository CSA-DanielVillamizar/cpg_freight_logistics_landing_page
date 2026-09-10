using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace CPG.Application.Features.Rates.RecomputeLaneRateStatistics;

/// <summary>
/// Batch job, not a request-path handler: pulls delivered loads into memory and aggregates
/// in C# (percentiles are not reliably translatable to SQL across providers), then upserts
/// one row per (OriginZip3, DestinationZip3, ServiceType, Month) group.
/// </summary>
public sealed class RecomputeLaneRateStatisticsCommandHandler(
    IApplicationDbContext dbContext,
    IDateTimeProvider clock)
    : IRequestHandler<RecomputeLaneRateStatisticsCommand, RecomputeLaneRateStatisticsResponse>
{
    private const int LookbackMonths = 18;

    public async Task<RecomputeLaneRateStatisticsResponse> Handle(
        RecomputeLaneRateStatisticsCommand request,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var cutoff = now.AddMonths(-LookbackMonths);

        var deliveredLoads = await dbContext.Loads
            .AsNoTracking()
            .Where(l => l.Status == LoadStatus.Delivered && l.DeliveryAtUtc >= cutoff && l.DistanceMiles > 0)
            .Select(l => new
            {
                l.OriginZip,
                l.DestinationZip,
                l.ServiceType,
                l.DeliveryAtUtc,
                l.RateUsd,
                l.DistanceMiles,
            })
            .ToListAsync(cancellationToken);

        var groups = deliveredLoads
            .GroupBy(l => new
            {
                OriginZip3 = Zip3(l.OriginZip),
                DestinationZip3 = Zip3(l.DestinationZip),
                l.ServiceType,
                Month = l.DeliveryAtUtc.Month,
            });

        var existingStatistics = await dbContext.LaneRateStatistics.ToListAsync(cancellationToken);
        var groupsUpserted = 0;

        foreach (var group in groups)
        {
            var ratesPerMile = group
                .Select(l => l.RateUsd / l.DistanceMiles)
                .OrderBy(rate => rate)
                .ToList();

            var existing = existingStatistics.SingleOrDefault(s =>
                s.OriginZip3 == group.Key.OriginZip3
                && s.DestinationZip3 == group.Key.DestinationZip3
                && s.ServiceType == group.Key.ServiceType
                && s.Month == group.Key.Month);

            if (existing is null)
            {
                dbContext.LaneRateStatistics.Add(new LaneRateStatistic
                {
                    OriginZip3 = group.Key.OriginZip3,
                    DestinationZip3 = group.Key.DestinationZip3,
                    ServiceType = group.Key.ServiceType,
                    Month = group.Key.Month,
                    AvgRatePerMileUsd = Round(ratesPerMile.Average()),
                    P25RatePerMileUsd = Round(Percentile(ratesPerMile, 0.25)),
                    P75RatePerMileUsd = Round(Percentile(ratesPerMile, 0.75)),
                    SampleSize = ratesPerMile.Count,
                    ComputedAtUtc = now,
                });
            }
            else
            {
                existing.AvgRatePerMileUsd = Round(ratesPerMile.Average());
                existing.P25RatePerMileUsd = Round(Percentile(ratesPerMile, 0.25));
                existing.P75RatePerMileUsd = Round(Percentile(ratesPerMile, 0.75));
                existing.SampleSize = ratesPerMile.Count;
                existing.ComputedAtUtc = now;
            }

            groupsUpserted++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RecomputeLaneRateStatisticsResponse(groupsUpserted, now);
    }

    private static string Zip3(string zip) => zip.Length >= 3 ? zip[..3] : zip;

    private static decimal Round(decimal value) => Math.Round(value, 4, MidpointRounding.AwayFromZero);

    /// <summary>Linear-interpolation percentile over an already-sorted list.</summary>
    private static decimal Percentile(IReadOnlyList<decimal> sortedValues, double percentile)
    {
        if (sortedValues.Count == 0)
        {
            return 0m;
        }

        if (sortedValues.Count == 1)
        {
            return sortedValues[0];
        }

        var rank = percentile * (sortedValues.Count - 1);
        var lowerIndex = (int)Math.Floor(rank);
        var upperIndex = (int)Math.Ceiling(rank);

        if (lowerIndex == upperIndex)
        {
            return sortedValues[lowerIndex];
        }

        var weight = (decimal)(rank - lowerIndex);
        return sortedValues[lowerIndex] + ((sortedValues[upperIndex] - sortedValues[lowerIndex]) * weight);
    }
}
