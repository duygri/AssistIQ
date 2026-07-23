using AssistIQ.Application.Abstractions;
using AssistIQ.Domain.Usage;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Infrastructure.Persistence;

public sealed class UsageLogRepository(AssistIQDbContext dbContext) : IUsageLogRepository
{
    public async Task<IReadOnlyList<UsageLog>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.UsageLogs
            .AsNoTracking()
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<UsageLog> Items, int Total)> ListPagedAsync(
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = dbContext.UsageLogs.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(log => log.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        return (items, total);
    }
}

