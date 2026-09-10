using CPG.Api.Infrastructure;
using CPG.Application.Features.Billing.Disbursements;
using CPG.Application.Features.Carriers;
using CPG.Application.Features.Carriers.Register;
using CPG.Application.Features.Telemetry;
using CPG.Application.Features.Telemetry.RegisterDevice;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CPG.Api.Controllers;

/// <summary>Carrier self-service onboarding. Restricted to the <c>Carrier</c> role.</summary>
[Authorize(Policy = AuthorizationPolicies.CarrierOnly)]
public sealed class CarriersController(ISender sender, IConfiguration configuration) : ApiControllerBase
{
    /// <summary>
    /// Creates the authenticated carrier user's profile so they can accept loads and file
    /// compliance documents. One profile per account; the account starts
    /// <c>PendingCompliance</c>. Returns 409 if a profile already exists.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CarrierRegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CarrierRegistrationResponse>> Register(
        [FromBody] RegisterCarrierRequest request,
        CancellationToken cancellationToken)
    {
        var carrier = await sender.Send(RegisterCarrierCommand.FromRequest(request), cancellationToken);
        return Created($"/api/carriers/{carrier.CarrierId}", carrier);
    }

    /// <summary>
    /// Links an ELD hardware unit to the authenticated carrier so its webhook signature can be
    /// verified (T-SDD Epica 2A). Returns the webhook secret exactly once — copy it into the
    /// provider's dashboard now, CPG never shows it again.
    /// </summary>
    [HttpPost("telemetry-devices")]
    [ProducesResponseType(typeof(TelemetryDeviceRegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TelemetryDeviceRegistrationResponse>> RegisterTelemetryDevice(
        [FromBody] RegisterTelemetryDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var device = await sender.Send(RegisterTelemetryDeviceCommand.FromRequest(request), cancellationToken);
        return Created($"/api/webhooks/telemetry/{device.Provider}", device);
    }

    /// <summary>
    /// Starts (or resumes) Stripe Connect onboarding for the authenticated carrier so they can
    /// receive payouts (T-SDD Epica 2B). Returns a hosted onboarding link — CPG never collects
    /// KYC data directly. Returns 409 if the carrier already has an active Stripe account.
    /// </summary>
    [HttpPost("stripe-connect")]
    [ProducesResponseType(typeof(StripeConnectOnboardingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StripeConnectOnboardingResponse>> ConnectStripe(CancellationToken cancellationToken)
    {
        var appOrigin = Request.Headers.Origin.ToString() is { Length: > 0 } origin
            ? origin
            : configuration["Billing:AppBaseUrl"] ?? "http://localhost:5173";

        var response = await sender.Send(
            new CreateStripeConnectAccountCommand(
                RefreshUrl: $"{appOrigin}/carrier/settings/stripe-connect",
                ReturnUrl: $"{appOrigin}/carrier/payouts?onboarding=complete"),
            cancellationToken);

        return Created(response.AccountLinkUrl, response);
    }

    /// <summary>The authenticated carrier's payout ledger, newest first (T-SDD Epica 2B).</summary>
    [HttpGet("payouts")]
    [ProducesResponseType(typeof(IReadOnlyList<PayoutHistoryEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PayoutHistoryEntryResponse>>> GetPayouts(
        CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetCarrierPayoutHistoryQuery(), cancellationToken));
}
