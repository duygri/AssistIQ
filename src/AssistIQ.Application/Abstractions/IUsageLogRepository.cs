using AssistIQ.Domain.Usage;

namespace AssistIQ.Application.Abstractions;

public interface IUsageLogRepository
{
    Task<IReadOnlyList<UsageLog>> ListAsync(CancellationToken cancellationToken);

    Task<(IReadOnlyList<UsageLog> Items, int Total)> ListPagedAsync(int skip, int take, CancellationToken cancellationToken);
}

