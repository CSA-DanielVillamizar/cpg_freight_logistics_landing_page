using MediatR;

namespace CPG.Application.Features.Loads.Depart;

/// <summary>
/// The assigned carrier reports departure from the origin. Runs in one EF Core transaction:
/// the load moves from <c>Dispatched</c> to <c>InTransit</c> and an audit row is written.
/// </summary>
public sealed record MarkLoadInTransitCommand(Guid LoadId) : IRequest<LoadSummaryResponse>;
