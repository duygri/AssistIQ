using AssistIQ.Domain.Users;

namespace AssistIQ.Application.Abstractions;

public interface IJwtTokenService
{
    string CreateToken(AppUser user);
}
