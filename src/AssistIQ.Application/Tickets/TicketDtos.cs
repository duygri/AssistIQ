using AssistIQ.Application.Drafts;
using AssistIQ.Domain.Tickets;

namespace AssistIQ.Application.Tickets;

public sealed record TicketDto(
    Guid Id,
    string CustomerQuestion,
    string? CustomerName,
    string? CustomerEmail,
    TicketStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DraftedAt,
    DateTimeOffset? SentAt,
    DraftDto? LatestDraft,
    IReadOnlyList<DraftSummaryDto> DraftHistory);

public sealed record TicketSummaryDto(
    Guid Id,
    string CustomerQuestion,
    string? CustomerName,
    string? CustomerEmail,
    TicketStatus Status,
    DateTimeOffset CreatedAt);
