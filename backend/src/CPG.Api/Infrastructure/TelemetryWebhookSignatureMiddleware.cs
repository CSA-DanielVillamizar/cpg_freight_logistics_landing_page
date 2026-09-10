using System.Text;
using System.Text.Json;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CPG.Api.Infrastructure;

/// <summary>
/// Intercepts <c>POST /api/webhooks/telemetry/{provider}</c> before MediatR and rejects any
/// request whose HMAC signature does not match the registered device's secret (T-SDD Epica 2A,
/// ADR-03). Runs only on endpoints marked <see cref="RequireTelemetrySignatureAttribute"/>.
/// </summary>
public sealed class TelemetryWebhookSignatureMiddleware(RequestDelegate next)
{
    private static readonly TimeSpan MaxClockDrift = TimeSpan.FromMinutes(5);

    public async Task InvokeAsync(
        HttpContext context,
        IApplicationDbContext dbContext,
        IEnumerable<ITelemetrySignatureVerifier> verifiers,
        ILogger<TelemetryWebhookSignatureMiddleware> logger)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<RequireTelemetrySignatureAttribute>() is null)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (!context.Request.RouteValues.TryGetValue("provider", out var providerRaw)
            || !Enum.TryParse<TelemetryProvider>(providerRaw as string, ignoreCase: true, out var provider))
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Unknown telemetry provider.").ConfigureAwait(false);
            return;
        }

        var verifier = verifiers.FirstOrDefault(v => v.Provider == provider);
        if (verifier is null)
        {
            await WriteProblemAsync(
                context, StatusCodes.Status400BadRequest, $"No signature verifier is registered for provider '{provider}'.")
                .ConfigureAwait(false);
            return;
        }

        // Buffer + rewind so the raw body can be hashed here and still deserialized normally
        // by the MVC model binder further down the pipeline.
        context.Request.EnableBuffering();
        string rawBody;
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            rawBody = await reader.ReadToEndAsync(context.RequestAborted).ConfigureAwait(false);
        }
        context.Request.Body.Position = 0;

        var (deviceId, recordedAtUtc) = ExtractLookupFields(rawBody);
        if (deviceId is null)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "The payload is missing 'deviceId'.").ConfigureAwait(false);
            return;
        }

        if (recordedAtUtc is { } timestamp
            && (timestamp < DateTimeOffset.UtcNow - MaxClockDrift || timestamp > DateTimeOffset.UtcNow + MaxClockDrift))
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "The 'recordedAtUtc' timestamp is outside the accepted clock drift.")
                .ConfigureAwait(false);
            return;
        }

        var device = await dbContext.TelemetryDevices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Provider == provider && d.ExternalDeviceId == deviceId, context.RequestAborted)
            .ConfigureAwait(false);

        var signatureHeader = context.Request.Headers[verifier.SignatureHeaderName].ToString();

        if (device is null || !verifier.Verify(rawBody, signatureHeader, device.WebhookSecretHash))
        {
            logger.LogWarning("Rejected telemetry webhook for provider {Provider}: invalid signature.", provider);
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Invalid webhook signature.").ConfigureAwait(false);
            return;
        }

        await next(context).ConfigureAwait(false);
    }

    private static (string? DeviceId, DateTimeOffset? RecordedAtUtc) ExtractLookupFields(string rawBody)
    {
        try
        {
            using var document = JsonDocument.Parse(rawBody);
            var root = document.RootElement;

            var deviceId = root.TryGetProperty("deviceId", out var deviceIdElement)
                ? deviceIdElement.GetString()
                : null;

            DateTimeOffset? recordedAtUtc = root.TryGetProperty("recordedAtUtc", out var recordedAtElement)
                && recordedAtElement.TryGetDateTimeOffset(out var parsed)
                ? parsed
                : null;

            return (deviceId, recordedAtUtc);
        }
        catch (JsonException)
        {
            return (null, null);
        }
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(new
        {
            type = $"https://httpstatuses.io/{statusCode}",
            title = statusCode == StatusCodes.Status401Unauthorized ? "Invalid webhook signature" : "Invalid webhook payload",
            status = statusCode,
            detail,
            traceId = context.TraceIdentifier,
        });
    }
}

/// <summary>Marks a webhook action as requiring <see cref="TelemetryWebhookSignatureMiddleware"/> verification.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class RequireTelemetrySignatureAttribute : Attribute;
