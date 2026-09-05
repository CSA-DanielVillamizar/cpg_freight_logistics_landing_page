using MediatR;

namespace CPG.Application.Features.Billing.CreateCheckout;

/// <summary>
/// Opens (or re-uses) a Stripe Checkout session for one of the shipper's payable invoices and
/// returns the URL their browser should be sent to.
/// </summary>
public sealed record CreateInvoiceCheckoutCommand(Guid InvoiceId, string SuccessUrl, string CancelUrl)
    : IRequest<InvoiceCheckoutResponse>;
