using CPG.Domain.Common;

namespace CPG.Domain.Events;

/// <summary>
/// Raised when a carrier accepts an available load from the board and it moves to
/// <see cref="Enums.LoadStatus.Dispatched"/>. Dispatched after the PostgreSQL transaction
/// commits; a handler forwards it to the message broker.
/// </summary>
public sealed record LoadAcceptedDomainEvent(Guid LoadId, string Reference, Guid CarrierId) : DomainEvent;
