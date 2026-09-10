using CPG.Application.Common.Interfaces;
using CPG.Application.Common.Messaging;
using CPG.Domain.Events;
using MediatR;

namespace CPG.Application.Features.Billing.Disbursements.EventHandlers;

/// <summary>
/// Bridges the committed <see cref="PaymentDisbursementCompletedDomainEvent"/> to a broker
/// integration event so notifications (email/SMS to the Carrier) can fire asynchronously
/// (T-SDD Epica 2B).
/// </summary>
public sealed class PaymentDisbursementCompletedDomainEventHandler(IEventBus eventBus)
    : INotificationHandler<PaymentDisbursementCompletedDomainEvent>
{
    public Task Handle(PaymentDisbursementCompletedDomainEvent notification, CancellationToken cancellationToken)
        => eventBus.PublishAsync(
            new QuickPayProcessedIntegrationEvent
            {
                LoadId = notification.LoadId,
                CarrierId = notification.CarrierId,
                CarrierNetAmountUsd = notification.CarrierNetAmountUsd,
                QuickPayRequested = notification.QuickPayRequested,
                StripeTransferId = notification.StripeTransferId,
            },
            cancellationToken);
}
