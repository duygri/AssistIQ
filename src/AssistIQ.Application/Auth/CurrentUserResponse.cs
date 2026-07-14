namespace AssistIQ.Application.Auth;

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string Role);
