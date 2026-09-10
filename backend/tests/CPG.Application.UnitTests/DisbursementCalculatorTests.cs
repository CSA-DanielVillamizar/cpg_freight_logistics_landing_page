using CPG.Application.Common.Interfaces;
using CPG.Application.Features.Billing.Disbursements;

namespace CPG.Application.UnitTests;

public sealed class DisbursementCalculatorTests
{
    private readonly DisbursementCalculator _sut = new();

    [Fact]
    public void Calculate_standard_load_deducts_only_the_cpg_margin()
    {
        // T-SDD Epica 2B worked example: $1000 gross, 5% margin, no agent, no Quick Pay.
        var result = _sut.Calculate(new DisbursementCalculationRequest
        {
            GrossAmountUsd = 1000m,
            AgentCommissionRatePercent = null,
            QuickPayRequested = false,
        });

        Assert.Equal(50.00m, result.CpgMarginAmountUsd);
        Assert.Equal(0m, result.AgentCommissionAmountUsd);
        Assert.Equal(0m, result.QuickPayFeeAmountUsd);
        Assert.Equal(950.00m, result.CarrierNetAmountUsd);
    }

    [Fact]
    public void Calculate_with_quick_pay_deducts_the_fee_after_the_margin()
    {
        // margin = 50.00; afterMargin = 950.00; quickPayFee = 3% * 950.00 = 28.50.
        var result = _sut.Calculate(new DisbursementCalculationRequest
        {
            GrossAmountUsd = 1000m,
            AgentCommissionRatePercent = null,
            QuickPayRequested = true,
        });

        Assert.Equal(50.00m, result.CpgMarginAmountUsd);
        Assert.Equal(0m, result.AgentCommissionAmountUsd);
        Assert.Equal(28.50m, result.QuickPayFeeAmountUsd);
        Assert.Equal(921.50m, result.CarrierNetAmountUsd);
    }

    [Fact]
    public void Calculate_with_agent_commission_deducts_it_after_the_margin()
    {
        // margin = 50.00; afterMargin = 950.00; agentCommission = 10% * 950.00 = 95.00.
        var result = _sut.Calculate(new DisbursementCalculationRequest
        {
            GrossAmountUsd = 1000m,
            AgentCommissionRatePercent = 10m,
            QuickPayRequested = false,
        });

        Assert.Equal(50.00m, result.CpgMarginAmountUsd);
        Assert.Equal(95.00m, result.AgentCommissionAmountUsd);
        Assert.Equal(0m, result.QuickPayFeeAmountUsd);
        Assert.Equal(855.00m, result.CarrierNetAmountUsd);
    }

    [Fact]
    public void Calculate_with_agent_and_quick_pay_stacks_both_fees_in_order()
    {
        // margin = 50.00; afterMargin = 950.00; agentCommission = 10% * 950.00 = 95.00;
        // afterAgent = 855.00; quickPayFee = 3% * 855.00 = 25.65.
        var result = _sut.Calculate(new DisbursementCalculationRequest
        {
            GrossAmountUsd = 1000m,
            AgentCommissionRatePercent = 10m,
            QuickPayRequested = true,
        });

        Assert.Equal(50.00m, result.CpgMarginAmountUsd);
        Assert.Equal(95.00m, result.AgentCommissionAmountUsd);
        Assert.Equal(25.65m, result.QuickPayFeeAmountUsd);
        Assert.Equal(829.35m, result.CarrierNetAmountUsd);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(33.33)]
    [InlineData(99.99)]
    [InlineData(333.33)]
    [InlineData(1234.56)]
    [InlineData(9999999.99)]
    public void Calculate_the_four_legs_always_sum_exactly_to_the_gross_amount(decimal gross)
    {
        foreach (var agentRate in new decimal?[] { null, 5m, 10m, 12.5m, 33.33m })
        {
            foreach (var quickPay in new[] { false, true })
            {
                var result = _sut.Calculate(new DisbursementCalculationRequest
                {
                    GrossAmountUsd = gross,
                    AgentCommissionRatePercent = agentRate,
                    QuickPayRequested = quickPay,
                });

                var sum = result.CpgMarginAmountUsd
                    + result.AgentCommissionAmountUsd
                    + result.QuickPayFeeAmountUsd
                    + result.CarrierNetAmountUsd;

                Assert.Equal(gross, sum);
            }
        }
    }

    [Fact]
    public void Calculate_zero_gross_produces_all_zero_legs()
    {
        var result = _sut.Calculate(new DisbursementCalculationRequest
        {
            GrossAmountUsd = 0m,
            AgentCommissionRatePercent = 10m,
            QuickPayRequested = true,
        });

        Assert.Equal(0m, result.CpgMarginAmountUsd);
        Assert.Equal(0m, result.AgentCommissionAmountUsd);
        Assert.Equal(0m, result.QuickPayFeeAmountUsd);
        Assert.Equal(0m, result.CarrierNetAmountUsd);
    }

    [Fact]
    public void Calculate_negative_gross_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Calculate(new DisbursementCalculationRequest
        {
            GrossAmountUsd = -1m,
            AgentCommissionRatePercent = null,
            QuickPayRequested = false,
        }));
    }

    [Fact]
    public void ApplyQuickPay_adds_the_fee_without_disturbing_the_already_struck_margin_and_agent_legs()
    {
        var initial = _sut.Calculate(new DisbursementCalculationRequest
        {
            GrossAmountUsd = 1000m,
            AgentCommissionRatePercent = 10m,
            QuickPayRequested = false,
        });

        var withQuickPay = _sut.ApplyQuickPay(initial, quickPayRequested: true);

        Assert.Equal(initial.CpgMarginAmountUsd, withQuickPay.CpgMarginAmountUsd);
        Assert.Equal(initial.AgentCommissionAmountUsd, withQuickPay.AgentCommissionAmountUsd);
        Assert.Equal(25.65m, withQuickPay.QuickPayFeeAmountUsd);
        Assert.Equal(829.35m, withQuickPay.CarrierNetAmountUsd);
    }

    [Fact]
    public void ApplyQuickPay_false_removes_the_fee_and_restores_the_full_net()
    {
        var withQuickPay = _sut.Calculate(new DisbursementCalculationRequest
        {
            GrossAmountUsd = 1000m,
            AgentCommissionRatePercent = null,
            QuickPayRequested = true,
        });

        var reverted = _sut.ApplyQuickPay(withQuickPay, quickPayRequested: false);

        Assert.Equal(0m, reverted.QuickPayFeeAmountUsd);
        Assert.Equal(950.00m, reverted.CarrierNetAmountUsd);
    }
}
