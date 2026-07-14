using AssistIQ.Api.Auth;
using AssistIQ.Application.Common;
using AssistIQ.Application.Drafts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.DraftsManage)]
public sealed class DraftsController(DraftService service) : ControllerBase
{
    [HttpPost("api/tickets/{ticketId:guid}/drafts/generate")]
    public async Task<ActionResult<DraftDto>> Generate(
        Guid ticketId,
        GenerateDraftRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.GenerateAsync(ticketId, request, cancellationToken));
        }
        catch (AppException exception)
        {
            return ToErrorResult(exception);
        }
    }

    [HttpPatch("api/drafts/{id:guid}")]
    public async Task<ActionResult<DraftDto>> Update(
        Guid id,
        UpdateDraftRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.UpdateAsync(id, request, cancellationToken));
        }
        catch (AppException exception)
        {
            return ToErrorResult(exception);
        }
    }

    [HttpPost("api/drafts/{id:guid}/send")]
    public async Task<ActionResult<DraftDto>> Send(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.SendAsync(id, cancellationToken));
        }
        catch (AppException exception)
        {
            return ToErrorResult(exception);
        }
    }

    private ActionResult ToErrorResult(AppException exception)
    {
        var body = new
        {
            errorCode = exception.ErrorCode,
            message = exception.Message
        };

        return exception.StatusCode switch
        {
            StatusCodes.Status400BadRequest => BadRequest(body),
            StatusCodes.Status403Forbidden => Forbid(),
            StatusCodes.Status404NotFound => NotFound(body),
            StatusCodes.Status409Conflict => Conflict(body),
            _ => StatusCode(exception.StatusCode, body)
        };
    }
}
