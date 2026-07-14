using AssistIQ.Domain.Users;

namespace AssistIQ.Application.Abstractions;

public interface ICurrentUser
{
    Guid UserId { get; }

    string Email { get; }

    UserRole Role { get; }
}
