using AssistIQ.Application.Abstractions;
using AssistIQ.Domain.Usage;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Infrastructure.Persistence;

public sealed class UsageLogRepository(AssistIQDbContext dbContext) : IUsageLogRepository
{
    public async Task<IReadOnlyList<UsageLog>> ListAsync(CancellationToken cancellationToken)
    {
        var logs = await dbContext.UsageLogs
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return logs
            .OrderByDescending(log => log.CreatedAt)
            .ToArray();
    }
}
