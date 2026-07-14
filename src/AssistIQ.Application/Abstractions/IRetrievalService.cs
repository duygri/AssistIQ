namespace AssistIQ.Application.Abstractions;

public interface IRetrievalService
{
    Task<IReadOnlyList<RetrievedSource>> RetrieveAsync(
        TicketRetrievalInput input,
        CancellationToken cancellationToken);
}

public sealed record TicketRetrievalInput(
    Guid TicketId,
    string CustomerQuestion);

public sealed record RetrievedSource(
    Guid KnowledgeDocumentId,
    string FileName,
    string ProviderFileId,
    string QuoteOrExcerpt,
    string? ProviderResultId,
    decimal? Confidence);
