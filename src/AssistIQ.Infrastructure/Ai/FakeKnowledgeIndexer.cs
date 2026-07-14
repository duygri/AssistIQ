using System.Security.Cryptography;
using System.Text;
using AssistIQ.Application.Abstractions;

namespace AssistIQ.Infrastructure.Ai;

public sealed class FakeKnowledgeIndexer : IKnowledgeIndexer
{
    public bool ShouldFailIndexing { get; set; }

    public bool ShouldFailDisable { get; set; }

    public Task<KnowledgeIndexResult> IndexAsync(
        string fileName,
        string contentType,
        long sizeBytes,
        string textContent,
        CancellationToken cancellationToken)
    {
        if (ShouldFailIndexing)
        {
            throw new InvalidOperationException("Fake indexer was configured to fail.");
        }

        if (string.IsNullOrWhiteSpace(textContent))
        {
            throw new InvalidOperationException("Text content is required for indexing.");
        }

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{fileName}:{sizeBytes}:{textContent}")))[..12]
            .ToLowerInvariant();

        return Task.FromResult(new KnowledgeIndexResult("vs_demo", $"file_{hash}"));
    }

    public Task DisableAsync(
        string providerVectorStoreId,
        string providerFileId,
        CancellationToken cancellationToken)
    {
        if (ShouldFailDisable)
        {
            throw new InvalidOperationException("Fake indexer disable was configured to fail.");
        }

        return Task.CompletedTask;
    }
}
