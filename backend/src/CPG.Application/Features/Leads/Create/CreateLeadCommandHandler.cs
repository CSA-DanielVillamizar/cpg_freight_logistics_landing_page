using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using MediatR;

namespace CPG.Application.Features.Leads.Create;

public sealed class CreateLeadCommandHandler(
    IApplicationDbContext dbContext,
    IDateTimeProvider clock)
    : IRequestHandler<CreateLeadCommand, CreateLeadResponse>
{
    public async Task<CreateLeadResponse> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = Lead.RegisterFromLandingPage(
            request.CompanyName,
            request.ContactName,
            request.ContactEmail,
            request.Phone,
            request.VerticalSlug,
            request.ServiceType,
            request.CargoDetails,
            clock.UtcNow);

        dbContext.Leads.Add(lead);

        // One commit; the CorporateLeadGeneratedDomainEvent is dispatched to RabbitMQ
        // afterwards by DispatchDomainEventsInterceptor.
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateLeadResponse
        {
            Id = lead.Id,
            Status = lead.Status,
        };
    }
}
