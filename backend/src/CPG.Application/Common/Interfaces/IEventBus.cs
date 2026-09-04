using CPG.Application.Common.Messaging;

namespace CPG.Application.Common.Interfaces;

/// <summary>
/// Publishes integration events to the message broker (RabbitMQ via MassTransit).
/// Used for asynchronous fan-out such as notifying the commercial team on a new lead
/// (SPEC.md US-04).
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}
