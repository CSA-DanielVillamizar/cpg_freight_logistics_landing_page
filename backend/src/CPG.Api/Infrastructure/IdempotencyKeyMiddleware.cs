using System.Text;
using CPG.Application.Common.Interfaces;
using Microsoft.Extensions.Primitives;

namespace CPG.Api.Infrastructure;

/// <summary>
/// Enforces and honours the <c>Idempotency-Key</c> header on unsafe write endpoints that
/// opt in via <see cref="RequireIdempotencyKeyAttribute"/> (SPEC.md section 2).
/// A repeated key replays the stored response instead of re-executing the operation.
/// </summary>
public sealed class IdempotencyKeyMiddleware(RequestDelegate next)
{
    public const string HeaderName = "Idempotency-Key";

    public async Task InvokeAsync(HttpContext context, IIdempotencyService idempotencyService)
    {
        var endpoint = context.GetEndpoint();
        var requiresKey = endpoint?.Metadata.GetMetadata<RequireIdempotencyKeyAttribute>() is not null;

        if (!requiresKey)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out StringValues headerValues)
            || StringValues.IsNullOrEmpty(headerValues))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Missing Idempotency-Key header",
                status = StatusCodes.Status400BadRequest,
                detail = $"Write requests to {context.Request.Path} require an '{HeaderName}: <UUID>' header.",
            }).ConfigureAwait(false);
            return;
        }

        var key = headerValues.ToString();

        var existing = await idempotencyService.TryGetResponseAsync(key, context.RequestAborted).ConfigureAwait(false);
        if (existing is not null)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            context.Response.Headers["Idempotency-Replayed"] = "true";
            await context.Response.WriteAsync(existing, Encoding.UTF8, context.RequestAborted).ConfigureAwait(false);
            return;
        }

        // Buffer the response so a successful outcome can be persisted for future replays.
        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await next(context).ConfigureAwait(false);

        buffer.Position = 0;
        var responseText = await new StreamReader(buffer).ReadToEndAsync(context.RequestAborted).ConfigureAwait(false);
        buffer.Position = 0;
        await buffer.CopyToAsync(originalBody, context.RequestAborted).ConfigureAwait(false);
        context.Response.Body = originalBody;

        if (context.Response.StatusCode is >= 200 and < 300)
        {
            await idempotencyService.StoreAsync(
                key,
                context.Request.Path,
                context.Response.StatusCode,
                responseText,
                context.RequestAborted).ConfigureAwait(false);
        }
    }
}

/// <summary>Marks an action/controller as requiring a valid <c>Idempotency-Key</c> header.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class RequireIdempotencyKeyAttribute : Attribute;
