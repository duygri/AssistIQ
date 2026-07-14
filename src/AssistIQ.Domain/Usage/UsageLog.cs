namespace AssistIQ.Domain.Usage;

public sealed class UsageLog
{
    private UsageLog()
    {
        Provider = string.Empty;
        Model = string.Empty;
    }

    private UsageLog(
        Guid id,
        Guid actorUserId,
        Guid? ticketId,
        Guid? draftId,
        string provider,
        string model,
        string? responseId,
        int promptTokens,
        int completionTokens,
        decimal estimatedCost,
        UsageStatus status,
        string? errorSummary,
        DateTimeOffset createdAt)
    {
        Id = id;
        ActorUserId = actorUserId;
        TicketId = ticketId;
        DraftId = draftId;
        Provider = provider;
        Model = model;
        ResponseId = responseId;
        PromptTokens = promptTokens;
        CompletionTokens = completionTokens;
        TotalTokens = promptTokens + completionTokens;
        EstimatedCost = estimatedCost;
        Status = status;
        ErrorSummary = errorSummary;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid ActorUserId { get; private set; }

    public Guid? TicketId { get; private set; }

    public Guid? DraftId { get; private set; }

    public string Provider { get; private set; }

    public string Model { get; private set; }

    public string? ResponseId { get; private set; }

    public int PromptTokens { get; private set; }

    public int CompletionTokens { get; private set; }

    public int TotalTokens { get; private set; }

    public decimal EstimatedCost { get; private set; }

    public UsageStatus Status { get; private set; }

    public string? ErrorSummary { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static UsageLog Succeeded(
        Guid actorUserId,
        Guid? ticketId,
        Guid? draftId,
        string provider,
        string model,
        string responseId,
        int promptTokens,
        int completionTokens,
        decimal estimatedCost,
        DateTimeOffset createdAt)
    {
        ValidateProvider(provider);
        ValidateModel(model);
        ValidateResponseId(responseId);
        ValidateTokenCounts(promptTokens, completionTokens);

        return new UsageLog(
            Guid.NewGuid(),
            actorUserId,
            ticketId,
            draftId,
            provider.Trim(),
            model.Trim(),
            responseId.Trim(),
            promptTokens,
            completionTokens,
            estimatedCost,
            UsageStatus.Succeeded,
            null,
            createdAt);
    }

    public static UsageLog Failed(
        Guid actorUserId,
        Guid? ticketId,
        Guid? draftId,
        string provider,
        string model,
        string errorSummary,
        DateTimeOffset createdAt)
    {
        ValidateProvider(provider);
        ValidateModel(model);

        if (string.IsNullOrWhiteSpace(errorSummary))
        {
            throw new InvalidOperationException("Usage error summary is required.");
        }

        return new UsageLog(
            Guid.NewGuid(),
            actorUserId,
            ticketId,
            draftId,
            provider.Trim(),
            model.Trim(),
            null,
            0,
            0,
            0m,
            UsageStatus.Failed,
            errorSummary.Trim(),
            createdAt);
    }

    private static void ValidateProvider(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new InvalidOperationException("Usage provider is required.");
        }
    }

    private static void ValidateModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            throw new InvalidOperationException("Usage model is required.");
        }
    }

    private static void ValidateResponseId(string responseId)
    {
        if (string.IsNullOrWhiteSpace(responseId))
        {
            throw new InvalidOperationException("Usage response id is required.");
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
