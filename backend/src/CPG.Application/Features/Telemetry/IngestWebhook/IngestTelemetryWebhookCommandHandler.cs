using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Telemetry.IngestWebhook;

public sealed class IngestTelemetryWebhookCommandHandler(
    IApplicationDbContext dbContext,
    IDateTimeProvider clock)
    : IRequestHandler<IngestTelemetryWebhookCommand>
{
    public async Task Handle(IngestTelemetryWebhookCommand request, CancellationToken cancellationToken)
    {
        var device = await dbContext.TelemetryDevices
            .FirstOrDefaultAsync(
                d => d.Provider == request.Provider && d.ExternalDeviceId == request.ExternalDeviceId,
                cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(TelemetryDevice), request.ExternalDeviceId);

        var load = await dbContext.Loads
            .FirstOrDefaultAsync(l => l.Id == request.LoadId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Load), request.LoadId);

        // Append-only history, independent of whether the reading is stale (see UpdateTelemetry
        // below) — the trail is a diagnostic record of everything the device ever reported.
        dbContext.TelemetryLogs.Add(new TelemetryLog
        {
            LoadId = load.Id,
            TelemetryDeviceId = device.Id,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            SpeedMph = request.SpeedMph,
            HeadingDegrees = request.HeadingDegrees,
            RecordedAtUtc = request.RecordedAtUtc,
            ReceivedAtUtc = clock.UtcNow,
        });

        // Idempotent: silently ignores out-of-order/duplicate readings and raises no event.
        load.UpdateTelemetry(request.Latitude, request.Longitude, request.RecordedAtUtc, request.SpeedMph);

        device.LastSeenAtUtc = clock.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
