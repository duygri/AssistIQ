using AssistIQ.Application.Auth;
using AssistIQ.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await authService.LoginAsync(request, cancellationToken));
        }
        catch (AppException exception) when (exception.StatusCode == StatusCodes.Status401Unauthorized)
        {
            return Unauthorized(new
            {
                errorCode = exception.ErrorCode,
                message = exception.Message
            });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await authService.GetCurrentUserAsync(cancellationToken));
        }
        catch (AppException exception) when (exception.StatusCode == StatusCodes.Status401Unauthorized)
        {
            return Unauthorized(new
            {
                errorCode = exception.ErrorCode,
                message = exception.Message
            });
        }
    }
}
