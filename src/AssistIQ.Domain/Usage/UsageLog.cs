namespace AssistIQ.Domain.Usage;

public sealed class UsageLog
{
    private UsageLog()
    {
        Provider = string.Empty;
    }

    private UsageLog(
        Guid id,
        Guid actorUserId,
        Guid? ticketId,
        Guid? draftId,
        string provider,
        int promptTokens,
        int completionTokens,
        UsageStatus status,
        string? failureCode,
        DateTimeOffset createdAt)
    {
        Id = id;
        ActorUserId = actorUserId;
        TicketId = ticketId;
        DraftId = draftId;
        Provider = provider;
        PromptTokens = promptTokens;
        CompletionTokens = completionTokens;
        TotalTokens = promptTokens + completionTokens;
        Status = status;
        FailureCode = failureCode;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid ActorUserId { get; private set; }

    public Guid? TicketId { get; private set; }

    public Guid? DraftId { get; private set; }

    public string Provider { get; private set; }

    public int PromptTokens { get; private set; }

    public int CompletionTokens { get; private set; }

    public int TotalTokens { get; private set; }

    public UsageStatus Status { get; private set; }

    public string? FailureCode { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static UsageLog Succeeded(
        Guid actorUserId,
        Guid? ticketId,
        Guid? draftId,
        string provider,
        int promptTokens,
        int completionTokens,
        DateTimeOffset createdAt)
    {
        ValidateProvider(provider);
        ValidateTokenCounts(promptTokens, completionTokens);

        return new UsageLog(
            Guid.NewGuid(),
            actorUserId,
            ticketId,
            draftId,
            provider.Trim(),
            promptTokens,
            completionTokens,
            UsageStatus.Succeeded,
            null,
            createdAt);
    }

    public static UsageLog Failed(
        Guid actorUserId,
        Guid? ticketId,
        Guid? draftId,
        string provider,
        string failureCode,
        DateTimeOffset createdAt)
    {
        ValidateProvider(provider);

        if (string.IsNullOrWhiteSpace(failureCode))
        {
            throw new InvalidOperationException("Usage failure code is required.");
        }

        return new UsageLog(
            Guid.NewGuid(),
            actorUserId,
            ticketId,
            draftId,
            provider.Trim(),
            0,
            0,
            UsageStatus.Failed,
            failureCode.Trim(),
            createdAt);
    }

    private static void ValidateProvider(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new InvalidOperationException("Usage provider is required.");
        }
    }

    private static void ValidateTokenCounts(int promptTokens, int completionTokens)
    {
        if (promptTokens < 0 || completionTokens < 0)
        {
            throw new InvalidOperationException("Usage token counts cannot be negative.");
        }
    }
}
