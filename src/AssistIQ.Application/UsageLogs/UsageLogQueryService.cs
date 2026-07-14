using AssistIQ.Application.Abstractions;

namespace AssistIQ.Application.UsageLogs;

public sealed class UsageLogQueryService(IUsageLogRepository repository)
{
    public async Task<IReadOnlyList<UsageLogDto>> ListAsync(CancellationToken cancellationToken)
    {
        var logs = await repository.ListAsync(cancellationToken);
        return logs.Select(log => new UsageLogDto(
            log.Id,
            log.ActorUserId,
            log.TicketId,
            log.DraftId,
            log.Provider,
            log.Model,
            log.ResponseId,
            log.PromptTokens,
            log.CompletionTokens,
            log.TotalTokens,
            log.EstimatedCost,
            log.Status,
            log.ErrorSummary,
            log.CreatedAt)).ToArray();
    }
}
