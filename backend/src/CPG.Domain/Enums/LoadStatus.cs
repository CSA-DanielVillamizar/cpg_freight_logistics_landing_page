namespace CPG.Domain.Enums;

/// <summary>Lifecycle of a freight load on the Carrier &amp; Shipper Load Workspace.</summary>
public enum LoadStatus
{
    Available = 1,
    Dispatched = 2,
    InTransit = 3,
    Delivered = 4,
}
