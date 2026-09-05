using CPG.Application.Common.Interfaces;
using CPG.Application.Common.Messaging;
using CPG.Domain.Events;
using MediatR;

namespace CPG.Application.Features.Loads.EventHandlers;

/// <summary>
/// Bridges the committed <see cref="LoadDeliveredDomainEvent"/> to a broker integration event
/// so the billing consumer can raise the shipper invoice asynchronously.
/// </summary>
public sealed class LoadDeliveredDomainEventHandler(IEventBus eventBus)
    : INotificationHandler<LoadDeliveredDomainEvent>
{
    public Task Handle(LoadDeliveredDomainEvent notification, CancellationToken cancellationToken)
        => eventBus.PublishAsync(
            new LoadDeliveredIntegrationEvent
            {
                LoadId = notification.LoadId,
                Reference = notification.Reference,
                ShipperUserId = notification.ShipperUserId,
            },
            cancellationToken);
}
