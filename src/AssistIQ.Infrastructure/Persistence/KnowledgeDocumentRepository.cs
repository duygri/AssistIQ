using AssistIQ.Application.Abstractions;
using AssistIQ.Domain.Knowledge;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Infrastructure.Persistence;

public sealed class KnowledgeDocumentRepository(AssistIQDbContext dbContext) : IKnowledgeDocumentRepository
{
    public async Task AddAsync(KnowledgeDocument document, CancellationToken cancellationToken)
    {
        await dbContext.KnowledgeDocuments.AddAsync(document, cancellationToken);
    }

    public Task<KnowledgeDocument?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.KnowledgeDocuments.FirstOrDefaultAsync(document => document.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<KnowledgeDocument>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.KnowledgeDocuments
            .AsNoTracking()
            .OrderByDescending(document => document.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<KnowledgeDocument> Items, int Total)> ListPagedAsync(
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = dbContext.KnowledgeDocuments.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(d => d.UploadedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

