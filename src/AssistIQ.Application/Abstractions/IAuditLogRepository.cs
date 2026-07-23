using AssistIQ.Domain.Audit;

namespace AssistIQ.Application.Abstractions;

public interface IAuditLogRepository
{
    Task<IReadOnlyList<AuditLog>> ListAsync(CancellationToken cancellationToken);

    Task<(IReadOnlyList<AuditLog> Items, int Total)> ListPagedAsync(int skip, int take, CancellationToken cancellationToken);
}
