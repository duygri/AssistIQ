using AssistIQ.Domain.Drafts;

namespace AssistIQ.Application.Drafts;

public sealed record DraftDto(
    Guid Id,
    Guid TicketId,
    int VersionNumber,
    DraftSource Source,
    DraftStatus Status,
    string GeneratedAnswer,
    string? EditedAnswer,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EditedAt,
    DateTimeOffset? SentAt,
    IReadOnlyList<DraftCitationDto> Citations);

public sealed record DraftSummaryDto(
    Guid Id,
    int VersionNumber,
    DraftStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SentAt);

public sealed record DraftCitationDto(
    Guid Id,
    Guid KnowledgeDocumentId,
    string FileName,
    string ProviderFileId,
    string Quote,
    string? ProviderResultId,
    decimal? Confidence);
