using CPG.Application.Common.Interfaces;
using CPG.Application.Common.Messaging;
using MassTransit;

namespace CPG.Infrastructure.Messaging;

/// <summary>
/// <see cref="IEventBus"/> implemented over MassTransit / RabbitMQ (SPEC.md section 1 + US-04).
/// </summary>
public sealed class MassTransitEventBus(IPublishEndpoint publishEndpoint) : IEventBus
{
    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
        => publishEndpoint.Publish(integrationEvent, cancellationToken);
}
