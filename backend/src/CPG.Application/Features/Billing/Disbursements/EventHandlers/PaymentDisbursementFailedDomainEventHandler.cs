using CPG.Application.Common.Interfaces;
using CPG.Application.Common.Messaging;
using CPG.Domain.Events;
using MediatR;

namespace CPG.Application.Features.Billing.Disbursements.EventHandlers;

/// <summary>
/// Bridges the committed <see cref="PaymentDisbursementFailedDomainEvent"/> to a broker
/// integration event so Admin/Finance can be alerted asynchronously (T-SDD Epica 2B).
/// </summary>
public sealed class PaymentDisbursementFailedDomainEventHandler(IEventBus eventBus)
    : INotificationHandler<PaymentDisbursementFailedDomainEvent>
{
    public Task Handle(PaymentDisbursementFailedDomainEvent notification, CancellationToken cancellationToken)
        => eventBus.PublishAsync(
            new PaymentDisbursementFailedIntegrationEvent
            {
                LoadId = notification.LoadId,
                CarrierId = notification.CarrierId,
                Reason = notification.Reason,
            },
            cancellationToken);
}
