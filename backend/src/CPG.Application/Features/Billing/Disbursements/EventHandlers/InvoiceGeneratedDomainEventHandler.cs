using CPG.Application.Common.Interfaces;
using CPG.Application.Common.Messaging;
using CPG.Domain.Events;
using MediatR;

namespace CPG.Application.Features.Billing.Disbursements.EventHandlers;

/// <summary>
/// Bridges the committed <see cref="InvoiceGeneratedDomainEvent"/> to a broker integration event
/// so the disbursement pipeline can pre-compute the Carrier's payout split (T-SDD Epica 2B).
/// </summary>
public sealed class InvoiceGeneratedDomainEventHandler(IEventBus eventBus)
    : INotificationHandler<InvoiceGeneratedDomainEvent>
{
    public Task Handle(InvoiceGeneratedDomainEvent notification, CancellationToken cancellationToken)
        => eventBus.PublishAsync(
            new InvoiceGeneratedIntegrationEvent
            {
                InvoiceId = notification.InvoiceId,
                LoadId = notification.LoadId,
                AmountUsd = notification.AmountUsd,
            },
            cancellationToken);
}
