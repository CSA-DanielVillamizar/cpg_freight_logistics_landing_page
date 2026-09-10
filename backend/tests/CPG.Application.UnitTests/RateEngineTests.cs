using System.Diagnostics;
using CPG.Application.Common.Interfaces;
using CPG.Application.Features.Rates;
using CPG.Application.Features.Rates.Engine;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CPG.Application.UnitTests;

public sealed class RateEngineTests
{
    private static RateEngine CreateEngine()
    {
        IServiceRateStrategy[] strategies =
        [
            new ColdChainRateStrategy(),
            new HeavyHaulRateStrategy(),
            new FlatbedRateStrategy(),
            new FdotConcreteRateStrategy(),
        ];

        return new RateEngine(strategies, new ZipCentroidDistanceCalculator(), new FixedClock());
    }

    [Fact]
    public void Cold_chain_quote_breaks_down_base_cold_chain_and_fuel()
    {
        var engine = CreateEngine();

        var response = engine.Calculate(new RateCalculationRequest
        {
            ServiceType = ServiceType.ColdChain,
            OriginZip = "33101",
            DestinationZip = "32801",
            WeightLbs = 35_000,
            TargetTemperatureCelsius = -20m,
        });

        response.BaseRate.Should().BeGreaterThan(0m);
        response.FuelSurcharge.Should().BeGreaterThan(0m);
        response.ColdChainSurcharge.Should().Be(350.00m); // 20C x 35000 lb x 0.0005
        response.TotalEstimated.Should()
            .Be(response.BaseRate + response.ColdChainSurcharge + response.FuelSurcharge);
        response.Currency.Should().Be("USD");
    }

    [Fact]
    public void Non_cold_chain_quote_has_zero_cold_chain_surcharge()
    {
        var engine = CreateEngine();

        var response = engine.Calculate(new RateCalculationRequest
        {
            ServiceType = ServiceType.HeavyHaul,
            OriginZip = "32801",
            DestinationZip = "30301",
            WeightLbs = 90_000,
        });

        response.ColdChainSurcharge.Should().Be(0m);
        response.FuelSurcharge.Should().BeGreaterThan(0m);
    }

    [Fact]
    public void Calculation_is_well_under_the_500ms_budget()
    {
        var engine = CreateEngine();
        var request = new RateCalculationRequest
        {
            ServiceType = ServiceType.Flatbed,
            OriginZip = "33602",
            DestinationZip = "31401",
            WeightLbs = 46_000,
        };

        engine.Calculate(request); // warm

        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 1_000; i++)
        {
            engine.Calculate(request);
        }

        stopwatch.Stop();
        (stopwatch.Elapsed.TotalMilliseconds / 1_000).Should().BeLessThan(1d);
    }
    [Fact]
    public void No_lane_statistic_falls_back_to_total_estimated_with_low_confidence()
    {
        var engine = CreateEngine();

        var response = engine.Calculate(new RateCalculationRequest
        {
            ServiceType = ServiceType.Flatbed,
            OriginZip = "33602",
            DestinationZip = "31401",
            WeightLbs = 46_000,
        });

        response.SuggestedRateUsd.Should().Be(response.TotalEstimated);
        response.SuggestedRateConfidence.Should().Be(RateConfidence.Low);
    }

    [Theory]
    [InlineData(5, RateConfidence.Low)]
    [InlineData(10, RateConfidence.Medium)]
    [InlineData(29, RateConfidence.Medium)]
    [InlineData(30, RateConfidence.High)]
    public void Lane_statistic_sample_size_drives_confidence_level(int sampleSize, RateConfidence expected)
    {
        var engine = CreateEngine();
        var request = new RateCalculationRequest
        {
            ServiceType = ServiceType.Flatbed,
            OriginZip = "33602",
            DestinationZip = "31401",
            WeightLbs = 46_000,
        };

        var statistic = new LaneRateStatistic
        {
            OriginZip3 = "336",
            DestinationZip3 = "314",
            ServiceType = ServiceType.Flatbed,
            Month = 9,
            AvgRatePerMileUsd = 2.50m,
            P25RatePerMileUsd = 2.10m,
            P75RatePerMileUsd = 2.90m,
            SampleSize = sampleSize,
            ComputedAtUtc = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero),
        };

        var response = engine.Calculate(request, statistic);

        response.SuggestedRateConfidence.Should().Be(expected);
        response.SuggestedRateUsd.Should().NotBeNull();
    }

    private sealed class FixedClock : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => new(2026, 9, 4, 0, 0, 0, TimeSpan.Zero);
    }
}
