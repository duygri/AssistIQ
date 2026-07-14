using AssistIQ.Domain.Knowledge;

namespace AssistIQ.Application.Knowledge;

public sealed record KnowledgeDocumentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    KnowledgeDocumentStatus Status,
    string? ProviderVectorStoreId,
    string? ProviderFileId,
    string? ErrorSummary,
    DateTimeOffset UploadedAt,
    DateTimeOffset? IndexedAt,
    DateTimeOffset? DisabledAt);
