using AssistIQ.Application.Abstractions;
using AssistIQ.Domain.Tickets;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Infrastructure.Persistence;

public sealed class TicketRepository(AssistIQDbContext dbContext) : ITicketRepository
{
    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        await dbContext.Tickets.AddAsync(ticket, cancellationToken);
    }

    public Task<Ticket?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Tickets.FirstOrDefaultAsync(ticket => ticket.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Tickets
            .AsNoTracking()
            .OrderByDescending(ticket => ticket.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
