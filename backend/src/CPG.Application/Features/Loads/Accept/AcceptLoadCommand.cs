using MediatR;

namespace CPG.Application.Features.Loads.Accept;

/// <summary>
/// The authenticated carrier claims an available load. Runs in a single EF Core transaction:
/// the load moves to <c>Dispatched</c>, an audit row is written, and after commit a
/// <c>LoadAcceptedIntegrationEvent</c> is published to the broker.
/// </summary>
public sealed record AcceptLoadCommand(Guid LoadId) : IRequest<LoadSummaryResponse>;
