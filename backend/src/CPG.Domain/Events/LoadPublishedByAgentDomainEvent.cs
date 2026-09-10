using CPG.Domain.Common;

namespace CPG.Domain.Events;

/// <summary>Raised when an Independent Agent publishes a load attributed to their own agency (T-SDD Epica 4).</summary>
public sealed record LoadPublishedByAgentDomainEvent(Guid LoadId, Guid AgentId, decimal ProjectedCommissionUsd)
    : DomainEvent;
