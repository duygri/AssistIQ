using AssistIQ.Domain.Usage;

namespace AssistIQ.Application.Abstractions;

public interface IUsageLogRepository
{
    Task<IReadOnlyList<UsageLog>> ListAsync(CancellationToken cancellationToken);
}
