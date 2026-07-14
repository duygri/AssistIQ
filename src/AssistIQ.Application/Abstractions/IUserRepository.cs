using AssistIQ.Domain.Users;

namespace AssistIQ.Application.Abstractions;

public interface IUserRepository
{
    Task<AppUser?> FindActiveByEmailAsync(string email, CancellationToken cancellationToken);

    Task<AppUser?> FindActiveByIdAsync(Guid id, CancellationToken cancellationToken);
}
