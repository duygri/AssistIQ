namespace AssistIQ.Domain.Users;

public sealed class AppUser
{
    private AppUser()
    {
        Email = string.Empty;
        DisplayName = string.Empty;
        PasswordHash = string.Empty;
    }

    private AppUser(Guid id, string email, string displayName, UserRole role, DateTimeOffset createdAt)
    {
        Id = id;
        Email = email;
        DisplayName = displayName;
        Role = role;
        IsActive = true;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; }

    public string DisplayName { get; private set; }

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public string PasswordHash { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? DisabledAt { get; private set; }

    public static AppUser Create(string email, string displayName, UserRole role, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("User email is required.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new InvalidOperationException("User display name is required.");
        }

        return new AppUser(Guid.NewGuid(), email.Trim(), displayName.Trim(), role, createdAt);
    }

    public void Disable(DateTimeOffset disabledAt)
    {
        IsActive = false;
        DisabledAt = disabledAt;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new InvalidOperationException("Password hash is required.");
        }

        PasswordHash = passwordHash;
    }
}
