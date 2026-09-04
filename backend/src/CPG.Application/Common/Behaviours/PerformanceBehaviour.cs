using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CPG.Application.Common.Behaviours;

/// <summary>
/// Warns when a request exceeds the latency budget. The rate calculator must stay under
/// 500&#160;ms (SPEC.md US-02); this behaviour makes regressions visible in the logs.
/// </summary>
public sealed class PerformanceBehaviour<TRequest, TResponse>(
    ILogger<PerformanceBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long SlowRequestThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next().ConfigureAwait(false);
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > SlowRequestThresholdMs)
        {
            logger.LogWarning(
                "Slow request {RequestName} took {ElapsedMs} ms (budget {BudgetMs} ms)",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds,
                SlowRequestThresholdMs);
        }

        return response;
    }
}
