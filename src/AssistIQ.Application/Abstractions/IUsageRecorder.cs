using AssistIQ.Domain.Usage;

namespace AssistIQ.Application.Abstractions;

public interface IUsageRecorder
{
    Task<UsageLog> RecordSucceededAsync(
        Guid ticketId,
        Guid? draftId,
        Guid actorUserId,
        string provider,
        string model,
        string responseId,
        int inputTokens,
        int outputTokens,
        CancellationToken cancellationToken);

    Task<UsageLog> RecordFailedAsync(
        Guid ticketId,
        Guid actorUserId,
        string provider,
        string model,
        string errorSummary,
        CancellationToken cancellationToken);
}
