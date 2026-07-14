using AssistIQ.Domain.Audit;

namespace AssistIQ.Application.Abstractions;

public interface IAuditService
{
    Task RecordAsync(
        Guid? actorUserId,
        AuditAction action,
        string entityType,
        Guid entityId,
        object? before,
        object? after,
        CancellationToken cancellationToken);
}
