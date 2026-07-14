using AssistIQ.Application.Abstractions;
using AssistIQ.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Infrastructure.Persistence;

public sealed class UserRepository(AssistIQDbContext dbContext) : IUserRepository
{
    public Task<AppUser?> FindActiveByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email && user.IsActive, cancellationToken);
    }

    public Task<AppUser?> FindActiveByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id && user.IsActive, cancellationToken);
    }
}
