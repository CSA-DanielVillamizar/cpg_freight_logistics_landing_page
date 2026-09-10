using CPG.Application.Common.Interfaces;

namespace CPG.Application.Features.Billing.Disbursements;

/// <inheritdoc cref="IDisbursementCalculator"/>
public sealed class DisbursementCalculator : IDisbursementCalculator
{
    public DisbursementBreakdown Calculate(DisbursementCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.GrossAmountUsd < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request), request.GrossAmountUsd, "Gross amount cannot be negative.");
        }

        var cpgMargin = Round(request.GrossAmountUsd * request.CpgMarginRatePercent / 100m);
        var afterMargin = request.GrossAmountUsd - cpgMargin;

        var agentCommission = request.AgentCommissionRatePercent is { } agentRate
            ? Round(afterMargin * agentRate / 100m)
            : 0m;
        var afterAgentCommission = afterMargin - agentCommission;

        var quickPayFee = request.QuickPayRequested
            ? Round(afterAgentCommission * request.QuickPayFeeRatePercent / 100m)
            : 0m;

        // The net leg is the exact remainder of the three already-rounded legs above, not an
        // independently rounded subtraction — this is what guarantees the four amounts always
        // sum to GrossAmountUsd to the penny.
        var carrierNet = request.GrossAmountUsd - cpgMargin - agentCommission - quickPayFee;

        return new DisbursementBreakdown
        {
            GrossAmountUsd = request.GrossAmountUsd,
            CpgMarginAmountUsd = cpgMargin,
            AgentCommissionAmountUsd = agentCommission,
            QuickPayFeeAmountUsd = quickPayFee,
            CarrierNetAmountUsd = carrierNet,
        };
    }

    public DisbursementBreakdown ApplyQuickPay(
        DisbursementBreakdown existing, bool quickPayRequested, decimal quickPayFeeRatePercent = 3.0m)
    {
        ArgumentNullException.ThrowIfNull(existing);

        var afterAgentCommission =
            existing.GrossAmountUsd - existing.CpgMarginAmountUsd - existing.AgentCommissionAmountUsd;

        var quickPayFee = quickPayRequested ? Round(afterAgentCommission * quickPayFeeRatePercent / 100m) : 0m;

        var carrierNet = existing.GrossAmountUsd
            - existing.CpgMarginAmountUsd
            - existing.AgentCommissionAmountUsd
            - quickPayFee;

        return existing with { QuickPayFeeAmountUsd = quickPayFee, CarrierNetAmountUsd = carrierNet };
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
