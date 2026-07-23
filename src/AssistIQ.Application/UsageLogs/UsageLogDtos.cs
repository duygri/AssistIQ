using AssistIQ.Domain.Usage;

namespace AssistIQ.Application.UsageLogs;

public sealed record UsageLogDto(
    Guid Id,
    Guid ActorUserId,
    Guid? TicketId,
    Guid? DraftId,
    string Provider,
    string Model,
    string? ResponseId,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    decimal EstimatedCost,
    UsageStatus Status,
    string? ErrorSummary,
    DateTimeOffset CreatedAt);

