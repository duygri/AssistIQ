namespace AssistIQ.Application.Abstractions;

public interface IKnowledgeIndexer
{
    Task<KnowledgeIndexResult> IndexAsync(
        string fileName,
        string contentType,
        long sizeBytes,
        string textContent,
        CancellationToken cancellationToken);

    Task DisableAsync(
        string providerVectorStoreId,
        string providerFileId,
        CancellationToken cancellationToken);
}

public sealed record KnowledgeIndexResult(
    string ProviderVectorStoreId,
    string ProviderFileId);
