using AssistIQ.Domain.Audit;

namespace AssistIQ.Application.Abstractions;

public interface IAuditLogRepository
{
    Task<IReadOnlyList<AuditLog>> ListAsync(CancellationToken cancellationToken);
}
