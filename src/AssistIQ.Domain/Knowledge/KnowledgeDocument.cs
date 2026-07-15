namespace AssistIQ.Domain.Knowledge;

public sealed class KnowledgeDocument
{
    private KnowledgeDocument()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        TextContent = string.Empty;
    }

    private KnowledgeDocument(
        Guid id,
        string fileName,
        string contentType,
        long sizeBytes,
        string textContent,
        Guid uploadedByUserId,
        DateTimeOffset uploadedAt)
    {
        Id = id;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        TextContent = textContent;
        UploadedByUserId = uploadedByUserId;
        UploadedAt = uploadedAt;
        Status = KnowledgeDocumentStatus.Indexing;
    }

    public Guid Id { get; private set; }

    public string FileName { get; private set; }

    public string ContentType { get; private set; }

    public long SizeBytes { get; private set; }

    public string TextContent { get; private set; }

    public Guid UploadedByUserId { get; private set; }

    public KnowledgeDocumentStatus Status { get; private set; }

    public string? ProviderVectorStoreId { get; private set; }

    public string? ProviderFileId { get; private set; }

    public string? ErrorSummary { get; private set; }

    public DateTimeOffset UploadedAt { get; private set; }

    public DateTimeOffset? IndexedAt { get; private set; }

    public DateTimeOffset? DisabledAt { get; private set; }

    public static KnowledgeDocument CreateIndexing(
        string fileName,
        string contentType,
        long sizeBytes,
        string textContent,
        Guid uploadedByUserId,
        DateTimeOffset uploadedAt)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException("Knowledge document file name is required.");
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new InvalidOperationException("Knowledge document content type is required.");
        }

        if (sizeBytes <= 0)
        {
            throw new InvalidOperationException("Knowledge document size must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(textContent))
        {
            throw new InvalidOperationException("Knowledge document text content is required.");
        }

        return new KnowledgeDocument(
            Guid.NewGuid(),
            fileName.Trim(),
            contentType.Trim(),
            sizeBytes,
            textContent.Trim(),
            uploadedByUserId,
            uploadedAt);
    }

    public void MarkReady(string providerVectorStoreId, string providerFileId, DateTimeOffset indexedAt)
    {
        if (Status == KnowledgeDocumentStatus.Disabled)
        {
            throw new InvalidOperationException("Disabled knowledge documents cannot be indexed.");
        }

        if (string.IsNullOrWhiteSpace(providerVectorStoreId))
        {
            throw new InvalidOperationException("Provider vector store id is required.");
        }

        if (string.IsNullOrWhiteSpace(providerFileId))
        {
            throw new InvalidOperationException("Provider file id is required.");
        }

        Status = KnowledgeDocumentStatus.Ready;
        ProviderVectorStoreId = providerVectorStoreId.Trim();
        ProviderFileId = providerFileId.Trim();
        IndexedAt = indexedAt;
        ErrorSummary = null;
    }

    public void MarkFailed(string errorSummary, DateTimeOffset failedAt)
    {
        if (Status == KnowledgeDocumentStatus.Disabled)
        {
            throw new InvalidOperationException("Disabled knowledge documents cannot be marked failed.");
        }

        if (string.IsNullOrWhiteSpace(errorSummary))
        {
            throw new InvalidOperationException("Knowledge document failure summary is required.");
        }

        Status = KnowledgeDocumentStatus.Failed;
        ErrorSummary = errorSummary.Trim();
        IndexedAt = failedAt;
    }

    public void Disable(DateTimeOffset disabledAt)
    {
        if (Status != KnowledgeDocumentStatus.Ready)
        {
            throw new InvalidOperationException("Only Ready knowledge documents can be Disabled.");
        }

        Status = KnowledgeDocumentStatus.Disabled;
        DisabledAt = disabledAt;
    }
}
