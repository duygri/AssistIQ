using AssistIQ.Application.Abstractions;

namespace AssistIQ.Application.AuditLogs;

public sealed class AuditLogQueryService(IAuditLogRepository repository)
{
    public async Task<IReadOnlyList<AuditLogDto>> ListAsync(CancellationToken cancellationToken)
    {
        var logs = await repository.ListAsync(cancellationToken);
        return logs.Select(log => new AuditLogDto(
            log.Id,
            log.ActorUserId,
            log.Action,
            log.EntityName,
            log.EntityId,
            log.OccurredAt,
            log.BeforeJson,
            log.AfterJson,
            log.MetadataJson)).ToArray();
    }
}
