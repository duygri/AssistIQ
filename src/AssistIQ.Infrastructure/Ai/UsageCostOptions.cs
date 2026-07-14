namespace AssistIQ.Infrastructure.Ai;

public sealed class UsageCostOptions
{
    public decimal DefaultInputCostPer1M { get; set; } = 1.25m;

    public decimal DefaultOutputCostPer1M { get; set; } = 10.0m;
}
