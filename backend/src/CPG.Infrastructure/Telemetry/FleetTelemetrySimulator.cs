using CPG.Application.Common.Interfaces;
using CPG.Application.Features.Telemetry;
using CPG.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CPG.Infrastructure.Telemetry;

/// <summary>
/// Background service that emits a dynamic (but fake) <see cref="TelemetryReading"/> every few
/// seconds for every load currently <see cref="LoadStatus.InTransit"/>. Position drifts along a
/// heading and cold-chain temperature random-walks inside its band with the occasional excursion,
/// so the Live Tracking UI has something to animate before a real GPS feed exists.
/// </summary>
public sealed class FleetTelemetrySimulator(
    IServiceScopeFactory scopeFactory,
    ITelemetryBroadcaster broadcaster,
    ILogger<FleetTelemetrySimulator> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(4);
    private readonly Dictionary<Guid, VehicleState> _state = [];
    private readonly Random _random = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Let migrations / seeding finish before the first query.
            await Task.Delay(TimeSpan.FromSeconds(6), stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        using var timer = new PeriodicTimer(Interval);
        while (!stoppingToken.IsCancellationRequested
               && await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                await TickAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fleet telemetry simulation tick failed");
            }
        }
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var loads = await dbContext.Loads
            .AsNoTracking()
            .Where(load => load.Status == LoadStatus.InTransit)
            .Select(load => new { load.Id, load.ServiceType, load.TargetTemperatureF })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var liveIds = loads.Select(load => load.Id).ToHashSet();
        foreach (var stale in _state.Keys.Where(id => !liveIds.Contains(id)).ToList())
        {
            _state.Remove(stale);
        }

        foreach (var load in loads)
        {
            if (!_state.TryGetValue(load.Id, out var vehicle))
            {
                vehicle = VehicleState.Seed(load.Id, load.ServiceType, load.TargetTemperatureF, _random);
                _state[load.Id] = vehicle;
            }

            vehicle.Advance(_random);

            await broadcaster.BroadcastAsync(
                new TelemetryReading
                {
                    LoadId = load.Id,
                    Latitude = Math.Round(vehicle.Latitude, 5),
                    Longitude = Math.Round(vehicle.Longitude, 5),
                    TemperatureCelsius = vehicle.TemperatureCelsius is { } celsius
                        ? Math.Round(celsius, 1)
                        : null,
                    SpeedMph = vehicle.SpeedMph,
                    TimestampUtc = DateTimeOffset.UtcNow,
                },
                cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private sealed class VehicleState
    {
        private const double SetpointFallbackCelsius = -18d;

        public double Latitude { get; private set; }

        public double Longitude { get; private set; }

        public int SpeedMph { get; private set; }

        public double? TemperatureCelsius { get; private set; }

        private double _headingRad;
        private double _setpointCelsius;
        private bool _coldChain;
        private int _excursionTicks;

        public static VehicleState Seed(Guid loadId, ServiceType serviceType, int? targetTempF, Random random)
        {
            // Deterministic-ish start point spread around Central Florida.
            var seed = loadId.GetHashCode();
            var latJitter = ((seed & 0xFF) / 255d - 0.5) * 2.4;
            var lngJitter = (((seed >> 8) & 0xFF) / 255d - 0.5) * 2.8;

            var coldChain = serviceType == ServiceType.ColdChain;
            var setpoint = targetTempF is { } f ? (f - 32d) * 5d / 9d : SetpointFallbackCelsius;

            return new VehicleState
            {
                Latitude = 28.9 + latJitter,
                Longitude = -82.6 + lngJitter,
                SpeedMph = random.Next(54, 66),
                _headingRad = random.NextDouble() * Math.PI * 2,
                _coldChain = coldChain,
                _setpointCelsius = coldChain ? setpoint : 0d,
                TemperatureCelsius = coldChain ? setpoint : null,
            };
        }

        public void Advance(Random random)
        {
            // Wander the heading a little, then step forward.
            _headingRad += (random.NextDouble() - 0.5) * 0.35;
            var stepDeg = 0.012 + random.NextDouble() * 0.01;
            Latitude += Math.Sin(_headingRad) * stepDeg;
            Longitude += Math.Cos(_headingRad) * stepDeg;

            SpeedMph = Math.Clamp(SpeedMph + random.Next(-3, 4), 38, 72);

            if (!_coldChain)
            {
                return;
            }

            var current = TemperatureCelsius ?? _setpointCelsius;

            if (_excursionTicks > 0)
            {
                _excursionTicks -= 1;
                current += 1.1; // climbing out of band
            }
            else
            {
                // Pull gently back toward setpoint + jitter.
                current += (_setpointCelsius - current) * 0.25 + (random.NextDouble() - 0.5) * 0.9;
                if (random.NextDouble() < 0.06)
                {
                    _excursionTicks = random.Next(3, 6); // door-seal fault
                }
            }

            TemperatureCelsius = Math.Clamp(current, -24d, -6d);
        }
    }
}
