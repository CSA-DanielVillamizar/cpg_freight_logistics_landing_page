using CPG.Domain.Common;

namespace CPG.Domain.Events;

/// <summary>Raised when an Admin approves an Agent's compliance review (T-SDD Epica 1 / Epica 4).</summary>
public sealed record AgentActivatedDomainEvent(Guid AgentId, Guid UserId) : DomainEvent;
