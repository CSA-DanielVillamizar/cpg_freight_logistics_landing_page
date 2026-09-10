using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Common;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Agents.AcceptInvitation;

public sealed class AcceptClientInvitationCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<AcceptClientInvitationCommand>
{
    public async Task Handle(AcceptClientInvitationCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var invitation = await dbContext.AgentClientInvitations
            .FirstOrDefaultAsync(i => i.Token == request.Token, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Invitation was not found.");

        if (invitation.Status != InvitationStatus.Sent)
        {
            throw new DomainException($"This invitation is no longer valid ({invitation.Status}).");
        }

        if (invitation.ExpiresAtUtc < clock.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            throw new DomainException("This invitation has expired.");
        }

        // The invitee must be signed in as the exact account the invitation was sent to —
        // prevents a leaked token being redeemed by an unrelated account.
        var email = currentUser.Email?.Trim().ToLowerInvariant();
        if (!string.Equals(email, invitation.InvitedEmail, StringComparison.Ordinal))
        {
            throw new ForbiddenAccessException();
        }

        invitation.AcceptedByUserId = userId;
        invitation.Status = InvitationStatus.Accepted;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
