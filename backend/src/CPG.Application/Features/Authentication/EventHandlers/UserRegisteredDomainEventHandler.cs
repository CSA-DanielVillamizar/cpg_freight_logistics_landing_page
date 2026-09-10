using CPG.Application.Common.Interfaces;
using CPG.Application.Common.Messaging;
using CPG.Domain.Events;
using MediatR;

namespace CPG.Application.Features.Authentication.EventHandlers;

/// <summary>
/// Bridges the committed <see cref="UserRegisteredDomainEvent"/> to a broker integration event
/// so the welcome email / CRM sync can be notified asynchronously (T-SDD Epica 1).
/// </summary>
public sealed class UserRegisteredDomainEventHandler(IEventBus eventBus)
    : INotificationHandler<UserRegisteredDomainEvent>
{
    public Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
        => eventBus.PublishAsync(
            new UserRegisteredIntegrationEvent
            {
                UserId = notification.UserId,
                Email = notification.Email,
                Role = notification.Role,
            },
            cancellationToken);
}
