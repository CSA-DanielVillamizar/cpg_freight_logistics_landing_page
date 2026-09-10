namespace CPG.Domain.Enums;

/// <summary>Lifecycle of a <see cref="Entities.PaymentDisbursement"/> ledger row (T-SDD Epica 2B).</summary>
public enum DisbursementStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
}
