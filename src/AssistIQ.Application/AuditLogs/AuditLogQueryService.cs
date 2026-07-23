using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Common;

namespace AssistIQ.Application.AuditLogs;

public sealed class AuditLogQueryService(IAuditLogRepository repository)
{
    public async Task<IReadOnlyList<AuditLogDto>> ListAsync(CancellationToken cancellationToken)
    {
        var logs = await repository.ListAsync(cancellationToken);
        return logs.Select(ToDto).ToArray();
    }

    public async Task<PagedResult<AuditLogDto>> ListPagedAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var (items, total) = await repository.ListPagedAsync(pagination.Skip, pagination.PageSize, cancellationToken);
        return new PagedResult<AuditLogDto>(items.Select(ToDto).ToArray(), total, pagination.Page, pagination.PageSize);
    }

    private static AuditLogDto ToDto(Domain.Audit.AuditLog log) =>
        new(log.Id, log.ActorUserId, log.Action, log.EntityName, log.EntityId, log.OccurredAt, log.BeforeJson, log.AfterJson, log.MetadataJson);
}

