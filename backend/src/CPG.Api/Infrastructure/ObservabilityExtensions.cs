using System.Diagnostics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CPG.Api.Infrastructure;

/// <summary>
/// Wires OpenTelemetry tracing. W3C <c>traceparent</c> propagation (SPEC.md section 2) is
/// on by default for ASP.NET Core + HttpClient; the OTLP exporter is added in a later phase
/// once a non-vulnerable package version aligns with the .NET 8 stack.
/// </summary>
public static class ObservabilityExtensions
{
    public const string ServiceName = "CPG.Api";

    /// <summary>Shared <see cref="ActivitySource"/> for hand-rolled spans across the API.</summary>
    public static readonly ActivitySource ActivitySource = new(ServiceName);

    public static IServiceCollection AddCpgObservability(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(ServiceName))
            .WithTracing(tracing => tracing
                .AddSource(ServiceName)
                .SetSampler(new AlwaysOnSampler())
                .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                .AddHttpClientInstrumentation()
                .AddConsoleExporter());

        return services;
    }
}
