using CPG.Application.Common.Interfaces;
using CPG.Application.Common.Messaging;
using CPG.Domain.Events;
using MediatR;

namespace CPG.Application.Features.Leads.EventHandlers;

/// <summary>
/// Bridges the committed <see cref="CorporateLeadGeneratedDomainEvent"/> to a RabbitMQ
/// integration event so the commercial team is notified asynchronously (SPEC.md US-04).
/// </summary>
public sealed class CorporateLeadGeneratedDomainEventHandler(IEventBus eventBus)
    : INotificationHandler<CorporateLeadGeneratedDomainEvent>
{
    public Task Handle(CorporateLeadGeneratedDomainEvent notification, CancellationToken cancellationToken)
        => eventBus.PublishAsync(
            new CorporateLeadGeneratedIntegrationEvent
            {
                LeadId = notification.LeadId,
                CompanyName = notification.CompanyName,
                ContactEmail = notification.ContactEmail,
                VerticalSlug = notification.VerticalSlug,
                ServiceType = notification.ServiceType,
            },
            cancellationToken);
}
