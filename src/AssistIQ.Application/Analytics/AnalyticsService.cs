namespace AssistIQ.Application.Analytics;

/// <summary>
/// Orchestrates aggregate queries to produce the admin stats response.
/// </summary>
public sealed class AnalyticsService(IAnalyticsRepository repository)
{
    private const int TopDocumentsCount = 5;

    public async Task<AdminStatsDto> GetAdminStatsAsync(CancellationToken cancellationToken)
    {
        var totalTickets = await repository.CountTicketsAsync(cancellationToken);
        var totalDrafts = await repository.CountDraftsAsync(cancellationToken);
        var sentDrafts = await repository.CountSentDraftsAsync(cancellationToken);
        var totalKnowledgeDocs = await repository.CountKnowledgeDocumentsAsync(cancellationToken);
        var readyKnowledgeDocs = await repository.CountReadyKnowledgeDocumentsAsync(cancellationToken);
        var (totalTokens, totalCost) = await repository.SumTokensAndCostAsync(cancellationToken);
        var topDocs = await repository.TopCitedDocumentsAsync(TopDocumentsCount, cancellationToken);

        var avgTokensPerDraft = totalDrafts == 0
            ? 0d
            : Math.Round((double)totalTokens / totalDrafts, 2);

        var topDocumentDtos = topDocs
            .Select(d => new TopDocumentDto(d.DocumentId, d.FileName, d.Count))
            .ToArray();

        return new AdminStatsDto(
            totalTickets,
            totalDrafts,
            sentDrafts,
            totalKnowledgeDocs,
            readyKnowledgeDocs,
            totalTokens,
            totalCost,
            avgTokensPerDraft,
            topDocumentDtos);
    }
}
