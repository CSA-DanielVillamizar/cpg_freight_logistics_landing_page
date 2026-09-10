using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Billing.Disbursements;

public sealed class ProcessLoadDisbursementCommandHandler(
    IApplicationDbContext dbContext,
    IDisbursementCalculator calculator,
    IStripeConnectService stripeConnect,
    IDateTimeProvider clock)
    : IRequestHandler<ProcessLoadDisbursementCommand>
{
    public async Task Handle(ProcessLoadDisbursementCommand request, CancellationToken cancellationToken)
    {
        var invoice = await dbContext.Invoices
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken)
            .ConfigureAwait(false);

        if (invoice is null)
        {
            return;
        }

        var disbursement = await dbContext.PaymentDisbursements
            .FirstOrDefaultAsync(d => d.InvoiceId == invoice.Id, cancellationToken)
            .ConfigureAwait(false);

        disbursement ??= await CreatePendingDisbursementAsync(invoice, cancellationToken).ConfigureAwait(false);

        if (disbursement is null)
        {
            return; // the load has no assigned carrier yet — nothing to disburse
        }

        if (invoice.Status != InvoiceStatus.Paid
            || disbursement.Status is DisbursementStatus.Completed or DisbursementStatus.Failed)
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var carrier = await dbContext.Carriers
            .FirstOrDefaultAsync(c => c.Id == disbursement.CarrierId, cancellationToken)
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(carrier?.StripeConnectAccountId))
        {
            disbursement.MarkFailed("StripeAccountNotConnected");
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var transferId = await stripeConnect
            .CreateTransferAsync(
                carrier.StripeConnectAccountId,
                disbursement.CarrierNetAmountUsd,
                $"CPG payout - invoice {invoice.Reference}",
                cancellationToken)
            .ConfigureAwait(false);

        if (transferId.StartsWith("tr_mock_", StringComparison.Ordinal))
        {
            // Mock gateway: no webhook will ever arrive, so settle synchronously (mirrors the
            // synchronous MockStripePaymentService Checkout flow).
            disbursement.MarkCompleted(transferId, clock.UtcNow);
        }
        else
        {
            // Real Stripe: settle once the transfer.paid webhook confirms it.
            disbursement.MarkTransferInitiated(transferId);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<PaymentDisbursement?> CreatePendingDisbursementAsync(
        Domain.Entities.Invoice invoice, CancellationToken cancellationToken)
    {
        var load = await dbContext.Loads
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Id == invoice.LoadId, cancellationToken)
            .ConfigureAwait(false);

        if (load?.AssignedCarrierId is not { } carrierId)
        {
            return null;
        }

        decimal? agentCommissionRate = null;
        if (load.AgentId is { } agentId)
        {
            agentCommissionRate = await dbContext.Agents
                .Where(a => a.Id == agentId)
                .Select(a => (decimal?)a.CommissionRatePercent)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        var breakdown = calculator.Calculate(new DisbursementCalculationRequest
        {
            GrossAmountUsd = invoice.AmountUsd,
            AgentCommissionRatePercent = agentCommissionRate,
            QuickPayRequested = false,
        });

        var disbursement = new PaymentDisbursement
        {
            LoadId = load.Id,
            InvoiceId = invoice.Id,
            CarrierId = carrierId,
            AgentId = load.AgentId,
            GrossAmountUsd = breakdown.GrossAmountUsd,
            CpgMarginAmountUsd = breakdown.CpgMarginAmountUsd,
            AgentCommissionAmountUsd = breakdown.AgentCommissionAmountUsd,
            QuickPayFeeAmountUsd = breakdown.QuickPayFeeAmountUsd,
            CarrierNetAmountUsd = breakdown.CarrierNetAmountUsd,
        };

        dbContext.PaymentDisbursements.Add(disbursement);
        return disbursement;
    }
}
