using System.ComponentModel.DataAnnotations;

namespace AssistIQ.Application.Auth;

public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(320)] string Email,
    [Required, StringLength(256)] string Password);
