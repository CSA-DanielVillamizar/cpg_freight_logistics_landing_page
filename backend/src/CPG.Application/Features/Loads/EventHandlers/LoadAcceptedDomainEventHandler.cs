using CPG.Application.Common.Interfaces;
using CPG.Application.Common.Messaging;
using CPG.Domain.Events;
using MediatR;

namespace CPG.Application.Features.Loads.EventHandlers;

/// <summary>
/// Bridges the committed <see cref="LoadAcceptedDomainEvent"/> to a broker integration event
/// so the dispatch desk is notified asynchronously.
/// </summary>
public sealed class LoadAcceptedDomainEventHandler(IEventBus eventBus)
    : INotificationHandler<LoadAcceptedDomainEvent>
{
    public Task Handle(LoadAcceptedDomainEvent notification, CancellationToken cancellationToken)
        => eventBus.PublishAsync(
            new LoadAcceptedIntegrationEvent
            {
                LoadId = notification.LoadId,
                Reference = notification.Reference,
                CarrierId = notification.CarrierId,
            },
            cancellationToken);
}
