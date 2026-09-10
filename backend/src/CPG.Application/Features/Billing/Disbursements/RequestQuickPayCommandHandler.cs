using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Common;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Billing.Disbursements;

public sealed class RequestQuickPayCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IDisbursementCalculator calculator)
    : IRequestHandler<RequestQuickPayCommand>
{
    public async Task Handle(RequestQuickPayCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var carrier = await dbContext.Carriers
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("A carrier profile is required to request Quick Pay.");

        var invoice = await dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        var load = await dbContext.Loads
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Id == invoice.LoadId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Load), invoice.LoadId);

        if (load.AssignedCarrierId != carrier.Id)
        {
            throw new ForbiddenAccessException();
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new DomainException("Invoice is already paid; Quick Pay is no longer available.");
        }

        var disbursement = await dbContext.PaymentDisbursements
            .FirstOrDefaultAsync(d => d.InvoiceId == invoice.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("No payout has been prepared for this invoice yet.");

        if (disbursement.Status is not DisbursementStatus.Pending)
        {
            throw new DomainException($"Quick Pay cannot be requested once the payout is {disbursement.Status}.");
        }

        disbursement.QuickPayRequested = true;

        var recalculated = calculator.ApplyQuickPay(
            new DisbursementBreakdown
            {
                GrossAmountUsd = disbursement.GrossAmountUsd,
                CpgMarginAmountUsd = disbursement.CpgMarginAmountUsd,
                AgentCommissionAmountUsd = disbursement.AgentCommissionAmountUsd,
                QuickPayFeeAmountUsd = disbursement.QuickPayFeeAmountUsd,
                CarrierNetAmountUsd = disbursement.CarrierNetAmountUsd,
            },
            quickPayRequested: true);

        disbursement.QuickPayFeeAmountUsd = recalculated.QuickPayFeeAmountUsd;
        disbursement.CarrierNetAmountUsd = recalculated.CarrierNetAmountUsd;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
