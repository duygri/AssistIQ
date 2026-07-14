using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Common;

namespace AssistIQ.Application.Auth;

public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    ICurrentUser currentUser)
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new AppException(401, ErrorCodes.Unauthorized, "Invalid email or password.");
        }

        var user = await users.FindActiveByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null || !passwordHasher.VerifyPassword(user, request.Password))
        {
            throw new AppException(401, ErrorCodes.Unauthorized, "Invalid email or password.");
        }

        return new LoginResponse(jwtTokenService.CreateToken(user), ToCurrentUserResponse(user));
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var user = await users.FindActiveByIdAsync(currentUser.UserId, cancellationToken);
        if (user is null)
        {
            throw new AppException(401, ErrorCodes.Unauthorized, "Current user was not found.");
        }

        return ToCurrentUserResponse(user);
    }

    private static CurrentUserResponse ToCurrentUserResponse(Domain.Users.AppUser user)
    {
        return new CurrentUserResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.Role.ToString());
    }
}
