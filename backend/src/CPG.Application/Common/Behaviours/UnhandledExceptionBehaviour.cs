using CPG.Application.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CPG.Application.Common.Behaviours;

/// <summary>
/// Last-resort logging for exceptions that are not part of the expected application
/// vocabulary (validation / not-found / forbidden). Re-throws for the API exception filter.
/// </summary>
public sealed class UnhandledExceptionBehaviour<TRequest, TResponse>(
    ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not ValidationException
            and not NotFoundException
            and not ForbiddenAccessException)
        {
            logger.LogError(ex, "Unhandled exception for request {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
