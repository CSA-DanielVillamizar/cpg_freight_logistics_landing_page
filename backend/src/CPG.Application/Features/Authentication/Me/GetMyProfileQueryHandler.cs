using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Authentication.Me;

public sealed class GetMyProfileQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetMyProfileQuery, MyProfileResponse>
{
    public async Task<MyProfileResponse> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(User), userId);

        CarrierProfileSummary? carrier = null;
        AgentProfileSummary? agent = null;

        if (user.Role == UserRole.Carrier)
        {
            var carrierEntity = await dbContext.Carriers
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
                .ConfigureAwait(false);

            if (carrierEntity is not null)
            {
                carrier = new CarrierProfileSummary
                {
                    CarrierId = carrierEntity.Id,
                    CompanyName = carrierEntity.CompanyName,
                    ComplianceStatus = carrierEntity.ComplianceStatus,
                };
            }
        }
        else if (user.Role == UserRole.Agent)
        {
            var agentEntity = await dbContext.Agents
                .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken)
                .ConfigureAwait(false);

            if (agentEntity is not null)
            {
                agent = new AgentProfileSummary
                {
                    AgentId = agentEntity.Id,
                    CompanyName = agentEntity.CompanyName,
                    Status = agentEntity.Status,
                    CommissionRatePercent = agentEntity.CommissionRatePercent,
                };
            }
        }

        return new MyProfileResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            CompanyName = user.CompanyName,
            PhoneNumber = user.PhoneNumber,
            Carrier = carrier,
            Agent = agent,
        };
    }
}
