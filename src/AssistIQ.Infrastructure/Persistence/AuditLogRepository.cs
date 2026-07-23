using AssistIQ.Application.Abstractions;
using AssistIQ.Domain.Audit;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Infrastructure.Persistence;

public sealed class AuditLogRepository(AssistIQDbContext dbContext) : IAuditLogRepository
{
    public async Task<IReadOnlyList<AuditLog>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(log => log.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<AuditLog> Items, int Total)> ListPagedAsync(
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = dbContext.AuditLogs.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(log => log.OccurredAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        return (items, total);
    }
}

