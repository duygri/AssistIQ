namespace AssistIQ.Application.Abstractions;

public interface IAiDraftService
{
    Task<AiDraftResult> GenerateAsync(
        TicketDraftInput input,
        CancellationToken cancellationToken);
}

public sealed record TicketDraftInput(
    Guid TicketId,
    string CustomerQuestion,
    string? Instructions,
    IReadOnlyList<RetrievedSource> Sources);

public sealed record AiDraftResult(
    string Answer,
    IReadOnlyList<AiCitationResult> Citations,
    string Provider,
    string Model,
    string ResponseId,
    int InputTokens,
    int OutputTokens);

public sealed record AiCitationResult(
    Guid KnowledgeDocumentId,
    string FileName,
    string ProviderFileId,
    string QuoteOrExcerpt,
    string? ProviderResultId,
    decimal? Confidence);
