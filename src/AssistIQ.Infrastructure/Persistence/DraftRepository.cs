using AssistIQ.Application.Abstractions;
using AssistIQ.Domain.Drafts;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Infrastructure.Persistence;

public sealed class DraftRepository(AssistIQDbContext dbContext) : IDraftRepository
{
    public async Task AddAsync(Draft draft, CancellationToken cancellationToken)
    {
        await dbContext.Drafts.AddAsync(draft, cancellationToken);
    }

    public Task<Draft?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Drafts
            .Include(draft => draft.Citations)
            .FirstOrDefaultAsync(draft => draft.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Draft>> ListByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        return await dbContext.Drafts
            .AsNoTracking()
            .Include(draft => draft.Citations)
            .Where(draft => draft.TicketId == ticketId)
            .OrderByDescending(draft => draft.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        return dbContext.Drafts.CountAsync(draft => draft.TicketId == ticketId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
