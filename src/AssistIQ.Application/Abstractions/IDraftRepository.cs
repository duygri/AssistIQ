using AssistIQ.Domain.Drafts;

namespace AssistIQ.Application.Abstractions;

public interface IDraftRepository
{
    Task AddAsync(Draft draft, CancellationToken cancellationToken);

    Task<Draft?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Draft>> ListByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken);

    Task<int> CountByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
