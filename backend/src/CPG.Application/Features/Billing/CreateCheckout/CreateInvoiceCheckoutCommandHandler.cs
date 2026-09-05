using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Common;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Billing.CreateCheckout;

public sealed class CreateInvoiceCheckoutCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IStripePaymentService stripe)
    : IRequestHandler<CreateInvoiceCheckoutCommand, InvoiceCheckoutResponse>
{
    public async Task<InvoiceCheckoutResponse> Handle(
        CreateInvoiceCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var invoice = await dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invoice '{request.InvoiceId}' was not found.");

        if (invoice.ShipperUserId != userId)
        {
            throw new ForbiddenAccessException("This invoice belongs to another shipper.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new DomainException($"Invoice {invoice.Reference} is already paid.");
        }

        var session = await stripe.CreateCheckoutSessionAsync(
            new StripeCheckoutRequest
            {
                InvoiceId = invoice.Id,
                InvoiceReference = invoice.Reference,
                AmountUsd = invoice.AmountUsd,
                CustomerEmail = currentUser.Email ?? "billing@cpgorlando.com",
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,
            },
            cancellationToken)
            .ConfigureAwait(false);

        invoice.AttachCheckoutSession(session.SessionId, session.CheckoutUrl);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new InvoiceCheckoutResponse { CheckoutUrl = session.CheckoutUrl };
    }
}
