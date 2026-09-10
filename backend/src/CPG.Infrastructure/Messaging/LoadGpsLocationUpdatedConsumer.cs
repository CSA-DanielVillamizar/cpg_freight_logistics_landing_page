using CPG.Application.Common.Interfaces;
using CPG.Application.Common.Messaging;
using CPG.Application.Features.Telemetry;
using MassTransit;

namespace CPG.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="LoadGpsLocationUpdatedIntegrationEvent"/> from the broker and forwards
/// it to the Shipper's live map over the <c>load-{loadId}</c> SignalR group (T-SDD Epica 2A).
/// </summary>
public sealed class LoadGpsLocationUpdatedConsumer(ITelemetryBroadcaster broadcaster)
    : IConsumer<LoadGpsLocationUpdatedIntegrationEvent>
{
    public Task Consume(ConsumeContext<LoadGpsLocationUpdatedIntegrationEvent> context)
    {
        var message = context.Message;

        return broadcaster.BroadcastToLoadGroupAsync(
            message.LoadId,
            new TelemetryReading
            {
                LoadId = message.LoadId,
                Latitude = (double)message.Latitude,
                Longitude = (double)message.Longitude,
                TemperatureCelsius = null,
                SpeedMph = message.SpeedMph is { } speed ? (int)Math.Round(speed) : 0,
                TimestampUtc = message.RecordedAtUtc,
            },
            context.CancellationToken);
    }
}
