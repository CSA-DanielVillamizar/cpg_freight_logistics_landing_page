using CPG.Application.Common.Interfaces;
using CPG.Application.Common.Messaging;
using CPG.Domain.Events;
using MediatR;

namespace CPG.Application.Features.Compliance.EventHandlers;

/// <summary>
/// Bridges the committed <see cref="ComplianceDocumentUploadedDomainEvent"/> to a RabbitMQ
/// integration event so the commercial/compliance team is notified asynchronously
/// (SPEC.md US-03).
/// </summary>
public sealed class ComplianceDocumentUploadedDomainEventHandler(IEventBus eventBus)
    : INotificationHandler<ComplianceDocumentUploadedDomainEvent>
{
    public Task Handle(ComplianceDocumentUploadedDomainEvent notification, CancellationToken cancellationToken)
        => eventBus.PublishAsync(
            new ComplianceDocumentUploadedIntegrationEvent
            {
                CarrierId = notification.CarrierId,
                DocumentId = notification.DocumentId,
                DocumentType = notification.DocumentType.ToString(),
            },
            cancellationToken);
}
