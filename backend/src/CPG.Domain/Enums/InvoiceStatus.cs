namespace CPG.Domain.Enums;

/// <summary>Lifecycle of a freight invoice billed to a corporate shipper.</summary>
public enum InvoiceStatus
{
    Draft = 1,
    Pending = 2,
    Paid = 3,
    Overdue = 4,

    /// <summary>Voided because the underlying load was deleted before payment.</summary>
    Cancelled = 5,
}
