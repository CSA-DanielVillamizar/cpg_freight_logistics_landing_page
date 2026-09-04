namespace CPG.Domain.Enums;

/// <summary>Lifecycle of a freight load on the board (SPEC.md US-03 / FDE idempotency).</summary>
public enum LoadStatus
{
    Draft = 1,
    Posted = 2,
    Assigned = 3,
    InTransit = 4,
    Delivered = 5,
    Cancelled = 6,
}
