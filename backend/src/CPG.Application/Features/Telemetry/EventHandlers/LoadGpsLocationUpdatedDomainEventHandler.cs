using CPG.Application.Common.Interfaces;
using CPG.Application.Common.Messaging;
using CPG.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Telemetry.EventHandlers;

/// <summary>
/// Bridges the committed <see cref="LoadGpsLocationUpdatedDomainEvent"/> to a broker integration
/// event so the Shipper's live map can be notified asynchronously (T-SDD Epica 2A).
/// </summary>
public sealed class LoadGpsLocationUpdatedDomainEventHandler(
    IEventBus eventBus,
    IApplicationDbContext dbContext)
    : INotificationHandler<LoadGpsLocationUpdatedDomainEvent>
{
    public async Task Handle(LoadGpsLocationUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var shipperUserId = await dbContext.Loads
            .AsNoTracking()
            .Where(load => load.Id == notification.LoadId)
            .Select(load => load.ShipperUserId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        await eventBus.PublishAsync(
            new LoadGpsLocationUpdatedIntegrationEvent
            {
                LoadId = notification.LoadId,
                Reference = notification.Reference,
                ShipperUserId = shipperUserId,
                Latitude = notification.Latitude,
                Longitude = notification.Longitude,
                SpeedMph = notification.SpeedMph,
                RecordedAtUtc = notification.RecordedAtUtc,
            },
            cancellationToken).ConfigureAwait(false);
    }
}
