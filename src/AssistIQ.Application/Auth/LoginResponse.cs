namespace AssistIQ.Application.Auth;

public sealed record LoginResponse(string Token, CurrentUserResponse User);
