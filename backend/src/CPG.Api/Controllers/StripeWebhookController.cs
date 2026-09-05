using System.Globalization;
using System.Net;
using System.Text;
using CPG.Application.Common.Interfaces;
using CPG.Application.Features.Billing.MarkInvoicePaid;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>
/// Stripe integration surface: the signed webhook that settles invoices asynchronously, and a
/// mock hosted-Checkout page used when no Stripe keys are configured.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api")]
public sealed class StripeWebhookController(ISender sender, IStripePaymentService stripe) : ControllerBase
{
    /// <summary>
    /// Receives <c>checkout.session.completed</c> from Stripe. Validates the signature, then
    /// marks the matching invoice paid (idempotent).
    /// </summary>
    [HttpPost("webhooks/stripe")]
    [Consumes("application/json")]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        var result = stripe.HandleWebhook(payload, signature);
        if (!result.SignatureValid)
        {
            return BadRequest(new { error = "Invalid signature." });
        }

        if (result.CheckoutCompleted && !string.IsNullOrEmpty(result.SessionId))
        {
            await sender.Send(new MarkInvoicePaidCommand(result.SessionId), cancellationToken);
        }

        return Ok(new { received = true });
    }

    /// <summary>Mock hosted Checkout page (only reachable when Stripe runs in mock mode).</summary>
    [HttpGet("billing/mock-checkout/{sessionId}")]
    public ContentResult MockCheckout(
        string sessionId,
        [FromQuery] string amount,
        [FromQuery] string reference,
        [FromQuery] string success,
        [FromQuery] string cancel)
    {
        var amountLabel = decimal.TryParse(amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value.ToString("C", CultureInfo.GetCultureInfo("en-US"))
            : amount;

        var html = $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>CPG Enterprises — Secure Checkout (mock)</title>
              <style>
                body { font-family: -apple-system, Segoe UI, Roboto, sans-serif; background: #0B192C; color: #0B1C30;
                       display: flex; min-height: 100vh; margin: 0; align-items: center; justify-content: center; }
                .card { background: #fff; border-radius: 12px; padding: 32px; width: 360px; box-shadow: 0 20px 60px rgba(0,0,0,.4); }
                h1 { font-size: 15px; text-transform: uppercase; letter-spacing: .08em; color: #64748B; margin: 0 0 8px; }
                .amount { font-size: 40px; font-weight: 800; margin: 8px 0 4px; }
                .ref { font-family: ui-monospace, monospace; color: #334155; font-size: 13px; margin-bottom: 24px; }
                button, a.btn { display: block; width: 100%; box-sizing: border-box; text-align: center; text-decoration: none;
                       font: inherit; font-weight: 600; padding: 12px; border-radius: 8px; border: 0; cursor: pointer; }
                .pay { background: #635BFF; color: #fff; margin-bottom: 10px; }
                .cancel { background: #F1F5F9; color: #334155; }
                .note { margin-top: 18px; font-size: 12px; color: #94A3B8; }
              </style>
            </head>
            <body>
              <div class="card">
                <h1>CPG Enterprises billing</h1>
                <div class="amount">{{WebUtility.HtmlEncode(amountLabel)}}</div>
                <div class="ref">{{WebUtility.HtmlEncode(reference)}} · session {{WebUtility.HtmlEncode(sessionId)}}</div>
                <form method="post" action="/api/billing/mock-checkout/{{Uri.EscapeDataString(sessionId)}}/complete">
                  <input type="hidden" name="success" value="{{WebUtility.HtmlEncode(success)}}" />
                  <button class="pay" type="submit">Pay {{WebUtility.HtmlEncode(amountLabel)}}</button>
                </form>
                <a class="btn cancel" href="{{WebUtility.HtmlEncode(cancel)}}">Cancel</a>
                <p class="note">Mock checkout — no card is charged. Swap in Stripe.net by configuring Stripe:SecretKey.</p>
              </div>
            </body>
            </html>
            """;

        return Content(html, "text/html");
    }

    /// <summary>Mock "payment succeeded" — settles the invoice and returns the browser to the SPA.</summary>
    [HttpPost("billing/mock-checkout/{sessionId}/complete")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> CompleteMockCheckout(
        string sessionId,
        [FromForm] string success,
        CancellationToken cancellationToken)
    {
        await sender.Send(new MarkInvoicePaidCommand(sessionId), cancellationToken);
        return Redirect(string.IsNullOrWhiteSpace(success) ? "/" : success);
    }
}
