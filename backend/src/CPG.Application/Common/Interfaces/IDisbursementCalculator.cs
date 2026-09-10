namespace CPG.Application.Common.Interfaces;

/// <summary>Inputs for splitting a delivered load's gross revenue (T-SDD Epica 2B).</summary>
public sealed record DisbursementCalculationRequest
{
    public required decimal GrossAmountUsd { get; init; }

    /// <summary>Null when the load was not published by an Independent Agent.</summary>
    public decimal? AgentCommissionRatePercent { get; init; }

    public required bool QuickPayRequested { get; init; }

    public decimal CpgMarginRatePercent { get; init; } = 5.0m;

    public decimal QuickPayFeeRatePercent { get; init; } = 3.0m;
}

/// <summary>
/// The four-way split of a load's gross revenue. The legs always sum exactly to
/// <see cref="GrossAmountUsd"/> — <see cref="CarrierNetAmountUsd"/> is the exact remainder
/// after the other three (already-rounded) legs, never independently rounded, so there is no
/// penny drift (T-SDD Epica 2B).
/// </summary>
public sealed record DisbursementBreakdown
{
    public required decimal GrossAmountUsd { get; init; }

    public required decimal CpgMarginAmountUsd { get; init; }

    public required decimal AgentCommissionAmountUsd { get; init; }

    public required decimal QuickPayFeeAmountUsd { get; init; }

    public required decimal CarrierNetAmountUsd { get; init; }
}

/// <summary>
/// Deterministic revenue-split calculator (T-SDD Epica 2B): CPG margin, an optional Agent
/// commission, an optional Quick Pay fee, and the Carrier's net payout. Pure and stateless —
/// no I/O — so it is safe to unit test exhaustively and reuse from both the disbursement
/// pipeline (Epica 2B) and the Agent's projected-commission preview (Epica 4).
/// </summary>
public interface IDisbursementCalculator
{
    DisbursementBreakdown Calculate(DisbursementCalculationRequest request);

    /// <summary>
    /// Re-derives <see cref="DisbursementBreakdown.QuickPayFeeAmountUsd"/> and
    /// <see cref="DisbursementBreakdown.CarrierNetAmountUsd"/> on an already-computed breakdown
    /// (the margin and Agent commission legs are fixed once struck) — used when a Carrier
    /// opts into Quick Pay after the initial split was recorded (T-SDD Epica 2B).
    /// </summary>
    DisbursementBreakdown ApplyQuickPay(
        DisbursementBreakdown existing, bool quickPayRequested, decimal quickPayFeeRatePercent = 3.0m);
}
