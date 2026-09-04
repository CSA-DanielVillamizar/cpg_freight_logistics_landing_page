using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CPG.Api.Infrastructure;

/// <summary>Registers the custom authorization result handler.</summary>
public static class AuthorizationResultHandlerExtensions
{
    public static IServiceCollection AddCpgAuthorizationResultHandler(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, CpgAuthorizationResultHandler>();
        return services;
    }
}

/// <summary>
/// Writes an RFC 7807 body on authorization failure. A forbidden (authenticated but wrong
/// role) response carries the message "Access denied" required by SPEC.md US-01 scenario 2.
/// </summary>
public sealed class CpgAuthorizationResultHandler(ProblemDetailsFactory problemDetailsFactory)
    : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _fallback = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden)
        {
            await WriteProblemAsync(context, StatusCodes.Status403Forbidden, "Access denied").ConfigureAwait(false);
            return;
        }

        if (authorizeResult.Challenged)
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Authentication required").ConfigureAwait(false);
            return;
        }

        await _fallback.HandleAsync(next, context, policy, authorizeResult).ConfigureAwait(false);
    }

    private async Task WriteProblemAsync(HttpContext context, int statusCode, string detail)
    {
        var problem = problemDetailsFactory.CreateProblemDetails(
            context,
            statusCode: statusCode,
            title: detail,
            detail: detail);

        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem).ConfigureAwait(false);
    }
}
