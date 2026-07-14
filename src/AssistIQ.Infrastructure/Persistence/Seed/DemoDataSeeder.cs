using AssistIQ.Application.Abstractions;
using AssistIQ.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Infrastructure.Persistence.Seed;

public sealed class DemoDataSeeder(
    AssistIQDbContext dbContext,
    IPasswordHasher passwordHasher,
    ISystemClock clock)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedUserAsync(
            "admin@assistiq.local",
            "AssistIQ Admin",
            UserRole.Admin,
            "Admin123!",
            cancellationToken);

        await SeedUserAsync(
            "agent@assistiq.local",
            "AssistIQ Agent",
            UserRole.SupportAgent,
            "Agent123!",
            cancellationToken);
    }

    private async Task SeedUserAsync(
        string email,
        string displayName,
        UserRole role,
        string password,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);
        if (exists)
        {
            return;
        }

        var user = AppUser.Create(email, displayName, role, clock.UtcNow);
        user.SetPasswordHash(passwordHasher.HashPassword(user, password));
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
