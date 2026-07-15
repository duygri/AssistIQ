using AssistIQ.Application.Abstractions;
using AssistIQ.Domain.Usage;
using AssistIQ.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace AssistIQ.Infrastructure.Ai;

public sealed class UsageRecorder(
    AssistIQDbContext dbContext,
    ISystemClock clock,
    IOptions<UsageCostOptions> options) : IUsageRecorder
{
    public async Task<UsageLog> RecordSucceededAsync(
        Guid ticketId,
        Guid? draftId,
        Guid actorUserId,
        string provider,
        string model,
        string responseId,
        int inputTokens,
        int outputTokens,
        CancellationToken cancellationToken)
    {
        var cost = CalculateCost(provider, inputTokens, outputTokens);
        var log = UsageLog.Succeeded(
            actorUserId,
            ticketId,
            draftId,
            provider,
            model,
            responseId,
            inputTokens,
            outputTokens,
            cost,
            clock.UtcNow);

        dbContext.UsageLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);
        return log;
    }

    public async Task<UsageLog> RecordFailedAsync(
        Guid ticketId,
        Guid actorUserId,
        string provider,
        string model,
        string errorSummary,
        CancellationToken cancellationToken)
    {
        var log = UsageLog.Failed(
            actorUserId,
            ticketId,
            draftId: null,
            provider,
            model,
            errorSummary,
            clock.UtcNow);

        dbContext.UsageLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);
        return log;
    }

    private decimal CalculateCost(string provider, int inputTokens, int outputTokens)
    {
        if (provider.Equals(GitHubModelsAiDraftService.ProviderName, StringComparison.OrdinalIgnoreCase))
        {
            return 0m;
        }

        var costOptions = options.Value;
        return (inputTokens / 1_000_000m * costOptions.DefaultInputCostPer1M)
            + (outputTokens / 1_000_000m * costOptions.DefaultOutputCostPer1M);
    }
}
