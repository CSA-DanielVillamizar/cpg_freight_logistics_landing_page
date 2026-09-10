using System.Security.Cryptography;
using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Application.Common.Messaging;
using CPG.Domain.Common;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Agents.InviteClient;

public sealed class InviteClientCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IDateTimeProvider clock,
    IEventBus eventBus)
    : IRequestHandler<InviteClientCommand, AgentClientInvitationResponse>
{
    public async Task<AgentClientInvitationResponse> Handle(
        InviteClientCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var agent = await dbContext.Agents
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("An agent profile is required to invite clients.");

        var email = request.InvitedEmail.Trim().ToLowerInvariant();

        var alreadyPending = await dbContext.AgentClientInvitations
            .AnyAsync(
                i => i.AgentId == agent.Id && i.InvitedEmail == email && i.Status == InvitationStatus.Sent,
                cancellationToken)
            .ConfigureAwait(false);

        if (alreadyPending)
        {
            throw new DomainException($"An invitation to '{email}' is already pending.");
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var now = clock.UtcNow;

        var invitation = new AgentClientInvitation
        {
            AgentId = agent.Id,
            InvitedEmail = email,
            Token = token,
            Status = InvitationStatus.Sent,
            ExpiresAtUtc = now.AddDays(7),
        };

        dbContext.AgentClientInvitations.Add(invitation);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // AgentClientInvitation is a plain Entity (not an AggregateRoot), so it cannot raise
        // domain events itself — publish the integration event directly, after commit.
        await eventBus.PublishAsync(
            new ClientInvitationSentIntegrationEvent
            {
                InvitationId = invitation.Id,
                AgentId = agent.Id,
                InvitedEmail = email,
                Token = token,
            },
            cancellationToken).ConfigureAwait(false);

        return new AgentClientInvitationResponse
        {
            InvitationId = invitation.Id,
            InvitedEmail = invitation.InvitedEmail,
            Status = invitation.Status,
            ExpiresAtUtc = invitation.ExpiresAtUtc,
        };
    }
}
