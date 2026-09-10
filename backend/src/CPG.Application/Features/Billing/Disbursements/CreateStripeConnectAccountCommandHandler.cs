using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Billing.Disbursements;

public sealed class CreateStripeConnectAccountCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IStripeConnectService stripeConnect)
    : IRequestHandler<CreateStripeConnectAccountCommand, StripeConnectOnboardingResponse>
{
    public async Task<StripeConnectOnboardingResponse> Handle(
        CreateStripeConnectAccountCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var carrier = await dbContext.Carriers
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("A carrier profile is required before connecting Stripe.");

        if (carrier.StripeOnboardingStatus == Domain.Enums.StripeOnboardingStatus.Active)
        {
            throw new DomainException("This carrier already has an active Stripe Connect account.");
        }

        if (string.IsNullOrEmpty(carrier.StripeConnectAccountId))
        {
            var email = currentUser.Email
                ?? throw new ForbiddenAccessException("The request is not authenticated.");

            carrier.StripeConnectAccountId = await stripeConnect
                .CreateConnectedAccountAsync(email, cancellationToken)
                .ConfigureAwait(false);
            carrier.StripeOnboardingStatus = Domain.Enums.StripeOnboardingStatus.PendingVerification;
        }

        var accountLinkUrl = await stripeConnect
            .CreateAccountLinkAsync(carrier.StripeConnectAccountId, request.RefreshUrl, request.ReturnUrl, cancellationToken)
            .ConfigureAwait(false);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new StripeConnectOnboardingResponse { AccountLinkUrl = accountLinkUrl };
    }
}
