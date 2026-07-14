using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Common;
using AssistIQ.Domain.Users;
using Microsoft.AspNetCore.Http;

namespace AssistIQ.Infrastructure.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var value = FindClaim(JwtRegisteredClaimNames.Sub, ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId)
                ? userId
                : throw new AppException(401, ErrorCodes.Unauthorized, "Current user id claim is missing.");
        }
    }

    public string Email => FindClaim(JwtRegisteredClaimNames.Email, ClaimTypes.Email)
        ?? throw new AppException(401, ErrorCodes.Unauthorized, "Current user email claim is missing.");

    public UserRole Role
    {
        get
        {
            var value = FindClaim(ClaimTypes.Role, "role");
            return Enum.TryParse<UserRole>(value, out var role)
                ? role
                : throw new AppException(401, ErrorCodes.Unauthorized, "Current user role claim is missing.");
        }
    }

    private string? FindClaim(params string[] claimTypes)
    {
        var user = httpContextAccessor.HttpContext?.User;
        return claimTypes
            .Select(type => user?.FindFirstValue(type))
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
