using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Common;
using AssistIQ.Application.Drafts;
using AssistIQ.Domain.Audit;
using AssistIQ.Domain.Drafts;
using AssistIQ.Domain.Tickets;
using AssistIQ.Domain.Users;

namespace AssistIQ.Application.Tickets;

public sealed class TicketService(
    ITicketRepository tickets,
    IDraftRepository drafts,
    IAuditService auditService,
    ICurrentUser currentUser,
    ISystemClock clock)
{
    public async Task<TicketDto> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerQuestion))
        {
            throw new AppException(400, ErrorCodes.ValidationFailed, "Customer question is required.");
        }

        var ticket = Ticket.Create(
            request.CustomerQuestion,
            request.CustomerName,
            request.CustomerEmail,
            currentUser.UserId,
            clock.UtcNow);

        await tickets.AddAsync(ticket, cancellationToken);
        await tickets.SaveChangesAsync(cancellationToken);

        var dto = await ToDtoAsync(ticket, cancellationToken);
        await auditService.RecordAsync(currentUser.UserId, AuditAction.TicketCreated, nameof(Ticket), ticket.Id, null, dto, cancellationToken);
        return dto;
    }

    public async Task<IReadOnlyList<TicketSummaryDto>> ListAsync(CancellationToken cancellationToken)
    {
        var allTickets = await tickets.ListAsync(cancellationToken);
        return allTickets
            .Where(CanAccess)
            .Select(ticket => new TicketSummaryDto(
                ticket.Id,
                ticket.CustomerQuestion,
                ticket.CustomerName,
                ticket.CustomerEmail,
                ticket.Status,
                ticket.CreatedAt))
            .ToArray();
    }

    public async Task<TicketDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await GetAccessibleTicketAsync(id, cancellationToken);
        return await ToDtoAsync(ticket, cancellationToken);
    }

    public async Task<Ticket> GetAccessibleTicketAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await tickets.FindByIdAsync(id, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, "Ticket was not found.");

        if (!CanAccess(ticket))
        {
            throw new AppException(403, ErrorCodes.Unauthorized, "You cannot access this ticket.");
        }

        return ticket;
    }

    private bool CanAccess(Ticket ticket)
    {
        return currentUser.Role == UserRole.Admin || ticket.CreatedByUserId == currentUser.UserId;
    }

    private async Task<TicketDto> ToDtoAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        var draftHistory = await drafts.ListByTicketIdAsync(ticket.Id, cancellationToken);
        var latestDraft = draftHistory.OrderByDescending(draft => draft.VersionNumber).FirstOrDefault();

        return new TicketDto(
            ticket.Id,
            ticket.CustomerQuestion,
            ticket.CustomerName,
            ticket.CustomerEmail,
            ticket.Status,
            ticket.CreatedAt,
            ticket.DraftedAt,
            ticket.SentAt,
            latestDraft is null ? null : ToDraftDto(latestDraft),
            draftHistory.Select(draft => new DraftSummaryDto(draft.Id, draft.VersionNumber, draft.Status, draft.CreatedAt, draft.SentAt)).ToArray());
    }

    private static DraftDto ToDraftDto(Draft draft)
    {
        return new DraftDto(
            draft.Id,
            draft.TicketId,
            draft.VersionNumber,
            draft.Source,
            draft.Status,
            draft.GeneratedAnswer,
            draft.EditedAnswer,
            draft.CreatedAt,
            draft.EditedAt,
            draft.SentAt,
            draft.Citations.Select(citation => new DraftCitationDto(
                citation.Id,
                citation.KnowledgeDocumentId,
                citation.FileName,
                citation.ProviderFileId,
                citation.Quote,
                citation.ProviderResultId,
                citation.Confidence)).ToArray());
    }
}
