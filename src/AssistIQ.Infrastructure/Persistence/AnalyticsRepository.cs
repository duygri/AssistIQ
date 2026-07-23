using AssistIQ.Application.Analytics;
using AssistIQ.Domain.Drafts;
using AssistIQ.Domain.Knowledge;
using AssistIQ.Domain.Usage;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Infrastructure.Persistence;

public sealed class AnalyticsRepository(AssistIQDbContext dbContext) : IAnalyticsRepository
{
    public Task<int> CountTicketsAsync(CancellationToken cancellationToken) =>
        dbContext.Tickets.CountAsync(cancellationToken);

    public Task<int> CountDraftsAsync(CancellationToken cancellationToken) =>
        dbContext.Drafts.CountAsync(cancellationToken);

    public Task<int> CountSentDraftsAsync(CancellationToken cancellationToken) =>
        dbContext.Drafts.CountAsync(d => d.Status == DraftStatus.Sent, cancellationToken);

    public Task<int> CountKnowledgeDocumentsAsync(CancellationToken cancellationToken) =>
        dbContext.KnowledgeDocuments.CountAsync(cancellationToken);

    public Task<int> CountReadyKnowledgeDocumentsAsync(CancellationToken cancellationToken) =>
        dbContext.KnowledgeDocuments.CountAsync(d => d.Status == KnowledgeDocumentStatus.Ready, cancellationToken);

    public async Task<(long TotalTokens, decimal TotalCost)> SumTokensAndCostAsync(CancellationToken cancellationToken)
    {
        var result = await dbContext.UsageLogs
            .AsNoTracking()
            .Where(u => u.Status == UsageStatus.Succeeded)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalTokens = (long)g.Sum(u => u.TotalTokens),
                TotalCost = g.Sum(u => u.EstimatedCost)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return result is null ? (0L, 0m) : (result.TotalTokens, result.TotalCost);
    }

    public async Task<IReadOnlyList<(Guid DocumentId, string FileName, int Count)>> TopCitedDocumentsAsync(
        int topN,
        CancellationToken cancellationToken)
    {
        var results = await dbContext.DraftCitations
            .AsNoTracking()
            .GroupBy(c => new { c.KnowledgeDocumentId, c.FileName })
            .Select(g => new
            {
                g.Key.KnowledgeDocumentId,
                g.Key.FileName,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(topN)
            .ToListAsync(cancellationToken);

        return results
            .Select(r => (r.KnowledgeDocumentId, r.FileName, r.Count))
            .ToArray();
    }
}
