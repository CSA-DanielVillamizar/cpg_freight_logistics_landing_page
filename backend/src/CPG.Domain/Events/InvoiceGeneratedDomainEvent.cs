using CPG.Domain.Common;

namespace CPG.Domain.Events;

/// <summary>Raised when a shipper invoice is raised for a delivered load (T-SDD Epica 2B).</summary>
public sealed record InvoiceGeneratedDomainEvent(Guid InvoiceId, Guid LoadId, decimal AmountUsd) : DomainEvent;
