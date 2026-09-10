using CPG.Application.Common.Messaging;
using CPG.Application.Features.Billing.Disbursements;
using MassTransit;
using MediatR;

namespace CPG.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="InvoiceGeneratedIntegrationEvent"/> and pre-computes the Carrier's
/// payout split for the load (T-SDD Epica 2B). The transfer itself only fires once the
/// shipper's invoice is paid — see <see cref="ProcessLoadDisbursementCommandHandler"/>.
/// </summary>
public sealed class InvoiceGeneratedConsumer(ISender sender) : IConsumer<InvoiceGeneratedIntegrationEvent>
{
    public Task Consume(ConsumeContext<InvoiceGeneratedIntegrationEvent> context)
        => sender.Send(new ProcessLoadDisbursementCommand(context.Message.InvoiceId), context.CancellationToken);
}
