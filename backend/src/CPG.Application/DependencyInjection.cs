using System.Reflection;
using CPG.Application.Common.Behaviours;
using CPG.Application.Features.Rates;
using CPG.Application.Features.Rates.Engine;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CPG.Application;

/// <summary>Composition root for the Application layer (CQRS + validation pipeline + rate engine).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });

        AddRateEngine(services);

        return services;
    }

    private static void AddRateEngine(IServiceCollection services)
    {
        // Strategy: one base-rate strategy per specialized service line.
        services.AddSingleton<IServiceRateStrategy, ColdChainRateStrategy>();
        services.AddSingleton<IServiceRateStrategy, HeavyHaulRateStrategy>();
        services.AddSingleton<IServiceRateStrategy, FlatbedRateStrategy>();
        services.AddSingleton<IServiceRateStrategy, FdotConcreteRateStrategy>();

        services.AddSingleton<IDistanceCalculator, ZipCentroidDistanceCalculator>();
        services.AddSingleton<IRateEngine, RateEngine>();
    }
}
