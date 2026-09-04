using CPG.Application.Features.Rates.Calculate;
using CPG.Domain.Enums;
using MediatR;

namespace CPG.Api.Infrastructure;

/// <summary>Startup warm-up so the first user-facing rate request is not paying JIT cost.</summary>
public static class WarmUpExtensions
{
    public static async Task WarmUpRateEngineAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        try
        {
            await sender.Send(new CalculateRateQuery(
                ServiceType.ColdChain,
                OriginZip: "33101",
                DestinationZip: "32801",
                WeightLbs: 1000,
                TargetTemperatureCelsius: -5m));
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("WarmUp");
            logger.LogWarning(ex, "Rate engine warm-up failed (non-fatal)");
        }
    }
}
