using AssistIQ.Application.Abstractions;
using AssistIQ.Domain.Audit;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Infrastructure.Persistence;

public sealed class AuditLogRepository(AssistIQDbContext dbContext) : IAuditLogRepository
{
    public async Task<IReadOnlyList<AuditLog>> ListAsync(CancellationToken cancellationToken)
    {
        var logs = await dbContext.AuditLogs
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return logs
            .OrderByDescending(log => log.OccurredAt)
            .ToArray();
    }
}
