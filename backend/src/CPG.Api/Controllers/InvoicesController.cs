using CPG.Api.Infrastructure;
using CPG.Application.Features.Billing.Disbursements;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Carrier-initiated actions against an invoice's payout (T-SDD Epica 2B).</summary>
[Authorize(Policy = AuthorizationPolicies.CarrierOnly)]
[Route("api/invoices")]
public sealed class InvoicesController(ISender sender) : ApiControllerBase
{
    /// <summary>
    /// The assigned carrier opts into Quick Pay: immediate settlement in exchange for a fee,
    /// instead of waiting for the shipper's net-30 terms. 409 if the invoice is already paid or
    /// the payout has moved past <c>Pending</c>, 403 if the load is assigned to another carrier,
    /// 404 if unknown.
    /// </summary>
    [HttpPost("{id:guid}/quick-pay")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequestQuickPay(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new RequestQuickPayCommand(id), cancellationToken).ConfigureAwait(false);
        return Accepted();
    }
}
