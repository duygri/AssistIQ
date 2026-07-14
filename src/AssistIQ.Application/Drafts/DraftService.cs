using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Common;
using AssistIQ.Application.Tickets;
using AssistIQ.Domain.Audit;
using AssistIQ.Domain.Drafts;
using AssistIQ.Domain.Tickets;

namespace AssistIQ.Application.Drafts;

public sealed class DraftService(
    TicketService ticketService,
    IDraftRepository drafts,
    IRetrievalService retrievalService,
    IAiDraftService aiDraftService,
    IUsageRecorder usageRecorder,
    IAuditService auditService,
    ICurrentUser currentUser)
{
    public async Task<DraftDto> GenerateAsync(Guid ticketId, GenerateDraftRequest request, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetAccessibleTicketAsync(ticketId, cancellationToken);
        var sources = await retrievalService.RetrieveAsync(
            new TicketRetrievalInput(ticket.Id, ticket.CustomerQuestion),
            cancellationToken);

        if (sources.Count == 0)
        {
            throw new AppException(409, ErrorCodes.NoReadyKnowledgeDocument, "At least one ready knowledge document is required.");
        }

        AiDraftResult result;
        try
        {
            result = await aiDraftService.GenerateAsync(
                new TicketDraftInput(ticket.Id, ticket.CustomerQuestion, request.Instructions, sources),
                cancellationToken);
        }
        catch (Exception exception)
        {
            await usageRecorder.RecordFailedAsync(ticket.Id, currentUser.UserId, "fake-ai", "unknown", exception.Message, cancellationToken);
            throw new AppException(502, ErrorCodes.IndexingFailed, "Draft generation failed.");
        }

        var citations = result.Citations.Select(citation => DraftCitation.Create(
            citation.KnowledgeDocumentId,
            citation.FileName,
            citation.ProviderFileId,
            citation.QuoteOrExcerpt,
            citation.ProviderResultId,
            citation.Confidence)).ToArray();

        var versionNumber = await drafts.CountByTicketIdAsync(ticket.Id, cancellationToken) + 1;
        var draft = Draft.CreateAiGenerated(ticket.Id, versionNumber, result.Answer, citations);

        await drafts.AddAsync(draft, cancellationToken);
        ticket.MarkDrafted(DateTimeOffset.UtcNow);
        await drafts.SaveChangesAsync(cancellationToken);

        await usageRecorder.RecordSucceededAsync(
            ticket.Id,
            draft.Id,
            currentUser.UserId,
            result.Provider,
            result.Model,
            result.ResponseId,
            result.InputTokens,
            result.OutputTokens,
            cancellationToken);

        var dto = ToDto(draft);
        await auditService.RecordAsync(currentUser.UserId, AuditAction.DraftGenerated, nameof(Draft), draft.Id, null, dto, cancellationToken);
        return dto;
    }

    public async Task<DraftDto> UpdateAsync(Guid id, UpdateDraftRequest request, CancellationToken cancellationToken)
    {
        var draft = await GetAccessibleDraftAsync(id, cancellationToken);
        draft.Edit(request.EditedAnswer);
        await drafts.SaveChangesAsync(cancellationToken);

        var dto = ToDto(draft);
        await auditService.RecordAsync(currentUser.UserId, AuditAction.DraftEdited, nameof(Draft), draft.Id, null, dto, cancellationToken);
        return dto;
    }

    public async Task<DraftDto> SendAsync(Guid id, CancellationToken cancellationToken)
    {
        var draft = await GetAccessibleDraftAsync(id, cancellationToken);
        var ticket = await ticketService.GetAccessibleTicketAsync(draft.TicketId, cancellationToken);

        try
        {
            draft.Send();
            ticket.MarkSent(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException exception)
        {
            throw new AppException(409, ErrorCodes.DraftNeedsCitationReview, exception.Message);
        }

        await drafts.SaveChangesAsync(cancellationToken);

        var dto = ToDto(draft);
        await auditService.RecordAsync(currentUser.UserId, AuditAction.DraftSent, nameof(Draft), draft.Id, null, dto, cancellationToken);
        return dto;
    }

    private async Task<Draft> GetAccessibleDraftAsync(Guid id, CancellationToken cancellationToken)
    {
        var draft = await drafts.FindByIdAsync(id, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, "Draft was not found.");

        await ticketService.GetAccessibleTicketAsync(draft.TicketId, cancellationToken);
        return draft;
    }

    private static DraftDto ToDto(Draft draft)
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
