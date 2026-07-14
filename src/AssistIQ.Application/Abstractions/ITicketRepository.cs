using AssistIQ.Domain.Tickets;

namespace AssistIQ.Application.Abstractions;

public interface ITicketRepository
{
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken);

    Task<Ticket?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Ticket>> ListAsync(CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
