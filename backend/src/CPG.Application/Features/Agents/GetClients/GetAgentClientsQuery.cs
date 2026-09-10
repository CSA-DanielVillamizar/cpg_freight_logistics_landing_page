using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Agents.GetClients;

/// <summary>The authenticated Agent's client invitations, newest first (T-SDD Epica 4).</summary>
public sealed record GetAgentClientsQuery : IRequest<IReadOnlyList<AgentClientView>>;

public sealed class GetAgentClientsQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetAgentClientsQuery, IReadOnlyList<AgentClientView>>
{
    public async Task<IReadOnlyList<AgentClientView>> Handle(
        GetAgentClientsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var agent = await dbContext.Agents
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("An agent profile is required to view clients.");

        return await dbContext.AgentClientInvitations
            .AsNoTracking()
            .Where(i => i.AgentId == agent.Id)
            .OrderByDescending(i => i.CreatedAtUtc)
            .Select(i => new AgentClientView
            {
                InvitationId = i.Id,
                InvitedEmail = i.InvitedEmail,
                Status = i.Status,
                InvitedAtUtc = i.CreatedAtUtc,
                ExpiresAtUtc = i.ExpiresAtUtc,
                AcceptedByUserId = i.AcceptedByUserId,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
