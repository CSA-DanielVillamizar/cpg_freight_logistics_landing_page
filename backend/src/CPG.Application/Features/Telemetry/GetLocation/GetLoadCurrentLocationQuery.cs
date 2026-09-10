using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Telemetry.GetLocation;

/// <summary>The last-known GPS position of a load, denormalized onto <see cref="Load"/> for fast reads (T-SDD Epica 2A).</summary>
public sealed record GetLoadCurrentLocationQuery(Guid LoadId) : IRequest<LoadLocationResponse>;

public sealed class GetLoadCurrentLocationQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetLoadCurrentLocationQuery, LoadLocationResponse>
{
    public async Task<LoadLocationResponse> Handle(
        GetLoadCurrentLocationQuery request,
        CancellationToken cancellationToken)
    {
        var load = await dbContext.Loads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.LoadId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Load), request.LoadId);

        return new LoadLocationResponse
        {
            LoadId = load.Id,
            Latitude = load.LastKnownLatitude,
            Longitude = load.LastKnownLongitude,
            LastTelemetryAtUtc = load.LastTelemetryAtUtc,
        };
    }
}
