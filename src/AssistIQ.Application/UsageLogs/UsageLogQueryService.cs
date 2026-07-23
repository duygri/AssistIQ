using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Common;

namespace AssistIQ.Application.UsageLogs;

public sealed class UsageLogQueryService(IUsageLogRepository repository)
{
    public async Task<IReadOnlyList<UsageLogDto>> ListAsync(CancellationToken cancellationToken)
    {
        var logs = await repository.ListAsync(cancellationToken);
        return logs.Select(ToDto).ToArray();
    }

    public async Task<PagedResult<UsageLogDto>> ListPagedAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var (items, total) = await repository.ListPagedAsync(pagination.Skip, pagination.PageSize, cancellationToken);
        return new PagedResult<UsageLogDto>(items.Select(ToDto).ToArray(), total, pagination.Page, pagination.PageSize);
    }

    private static UsageLogDto ToDto(Domain.Usage.UsageLog log) =>
        new(log.Id, log.ActorUserId, log.TicketId, log.DraftId, log.Provider, log.Model, log.ResponseId,
            log.PromptTokens, log.CompletionTokens, log.TotalTokens, log.EstimatedCost, log.Status, log.ErrorSummary, log.CreatedAt);
}

