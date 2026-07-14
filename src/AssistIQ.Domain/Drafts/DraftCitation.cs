namespace AssistIQ.Domain.Drafts;

public sealed class DraftCitation
{
    private DraftCitation()
    {
        FileName = string.Empty;
        ProviderFileId = string.Empty;
        Quote = string.Empty;
    }

    private DraftCitation(
        Guid id,
        Guid knowledgeDocumentId,
        string fileName,
        string providerFileId,
        string quote,
        string? providerResultId,
        decimal? confidence)
    {
        Id = id;
        KnowledgeDocumentId = knowledgeDocumentId;
        FileName = fileName;
        ProviderFileId = providerFileId;
        Quote = quote;
        ProviderResultId = providerResultId;
        Confidence = confidence;
    }

    public Guid Id { get; private set; }

    public Guid DraftId { get; private set; }

    public Guid KnowledgeDocumentId { get; private set; }

    public string FileName { get; private set; }

    public string ProviderFileId { get; private set; }

    public string Quote { get; private set; }

    public string? ProviderResultId { get; private set; }

    public decimal? Confidence { get; private set; }

    public static DraftCitation Create(
        Guid knowledgeDocumentId,
        string fileName,
        string providerFileId,
        string quote,
        string? providerResultId,
        decimal? confidence)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException("Draft citation file name is required.");
        }

        if (string.IsNullOrWhiteSpace(providerFileId))
        {
            throw new InvalidOperationException("Draft citation provider file id is required.");
        }

        if (string.IsNullOrWhiteSpace(quote))
        {
            throw new InvalidOperationException("Draft citation quote is required.");
        }

        return new DraftCitation(
            Guid.NewGuid(),
            knowledgeDocumentId,
            fileName.Trim(),
            providerFileId.Trim(),
            quote.Trim(),
            string.IsNullOrWhiteSpace(providerResultId) ? null : providerResultId.Trim(),
            confidence);
    }
}
