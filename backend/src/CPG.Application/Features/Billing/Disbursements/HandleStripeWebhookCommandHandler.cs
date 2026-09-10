using CPG.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Billing.Disbursements;

public sealed class HandleStripeWebhookCommandHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
    : IRequestHandler<HandleStripeWebhookCommand>
{
    private const string TransferPaid = "transfer.paid";
    private const string TransferFailed = "transfer.failed";

    public async Task Handle(HandleStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        var disbursement = await dbContext.PaymentDisbursements
            .FirstOrDefaultAsync(d => d.StripeTransferId == request.StripeTransferId, cancellationToken)
            .ConfigureAwait(false);

        if (disbursement is null)
        {
            return; // unrelated transfer (e.g. a different platform account) — ignore
        }

        switch (request.EventType)
        {
            case TransferPaid:
                if (disbursement.Status != Domain.Enums.DisbursementStatus.Completed)
                {
                    disbursement.MarkCompleted(request.StripeTransferId, clock.UtcNow);
                }
                break;

            case TransferFailed:
                if (disbursement.Status != Domain.Enums.DisbursementStatus.Failed)
                {
                    disbursement.MarkFailed(request.FailureReason ?? "StripeTransferFailed");
                }
                break;

            default:
                return;
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
