using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CPG.Application.Common.Behaviours;

/// <summary>
/// Structured request/response logging with the active W3C trace id attached, so every
/// CQRS operation is correlatable end to end (SPEC.md section 2 - distributed tracing).
/// </summary>
public sealed class LoggingBehaviour<TRequest, TResponse>(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var traceId = Activity.Current?.TraceId.ToString() ?? "none";

        logger.LogInformation("Handling {RequestName} [trace {TraceId}]", requestName, traceId);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next().ConfigureAwait(false);
            stopwatch.Stop();
            logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs} ms [trace {TraceId}]",
                requestName,
                stopwatch.ElapsedMilliseconds,
                traceId);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex,
                "Failed {RequestName} after {ElapsedMs} ms [trace {TraceId}]",
                requestName,
                stopwatch.ElapsedMilliseconds,
                traceId);
            throw;
        }
    }
}
