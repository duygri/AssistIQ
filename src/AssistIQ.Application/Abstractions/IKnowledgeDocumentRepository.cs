using AssistIQ.Domain.Knowledge;

namespace AssistIQ.Application.Abstractions;

public interface IKnowledgeDocumentRepository
{
    Task AddAsync(KnowledgeDocument document, CancellationToken cancellationToken);

    Task<KnowledgeDocument?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<KnowledgeDocument>> ListAsync(CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
