namespace AssistIQ.Application.Analytics;

/// <summary>
/// Top-level admin statistics response for the /api/admin/stats endpoint.
/// </summary>
public sealed record AdminStatsDto(
    int TotalTickets,
    int TotalDrafts,
    int SentDrafts,
    int TotalKnowledgeDocuments,
    int ReadyKnowledgeDocuments,
    long TotalTokensUsed,
    decimal TotalEstimatedCost,
    double AverageTokensPerDraft,
    IReadOnlyList<TopDocumentDto> TopCitedDocuments);

/// <summary>
/// A knowledge document cited by the AI, ranked by citation frequency.
/// </summary>
public sealed record TopDocumentDto(
    Guid KnowledgeDocumentId,
    string FileName,
    int CitationCount);
