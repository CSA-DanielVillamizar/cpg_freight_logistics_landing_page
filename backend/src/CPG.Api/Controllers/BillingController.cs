using CPG.Api.Infrastructure;
using CPG.Application.Features.Billing;
using CPG.Application.Features.Billing.CreateCheckout;
using CPG.Application.Features.Billing.GetShipperInvoices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CPG.Api.Controllers;

/// <summary>Shipper billing: invoices raised from delivered loads and Stripe Checkout.</summary>
[Authorize(Policy = AuthorizationPolicies.ShipperOnly)]
[Route("api/shipper/invoices")]
public sealed class BillingController(ISender sender, IConfiguration configuration) : ApiControllerBase
{
    /// <summary>The authenticated shipper's invoices, newest first, with the outstanding balance.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ShipperInvoicesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ShipperInvoicesResponse>> GetInvoices(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetShipperInvoicesQuery(), cancellationToken));

    /// <summary>
    /// Opens a Stripe Checkout session for a payable invoice and returns the URL to redirect to.
    /// 403 if the invoice belongs to another shipper, 404 if unknown, 409 if already paid.
    /// </summary>
    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(typeof(InvoiceCheckoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InvoiceCheckoutResponse>> Pay(Guid id, CancellationToken cancellationToken)
    {
        // Where Checkout sends the browser afterwards — the caller's own origin (the SPA), or config.
        var appOrigin = Request.Headers.Origin.ToString() is { Length: > 0 } origin
            ? origin
            : configuration["Billing:AppBaseUrl"] ?? "http://localhost:5173";

        var response = await sender.Send(
            new CreateInvoiceCheckoutCommand(
                id,
                SuccessUrl: $"{appOrigin}/shipper/billing?checkout=success",
                CancelUrl: $"{appOrigin}/shipper/billing?checkout=canceled"),
            cancellationToken);

        return Ok(response);
    }
}
