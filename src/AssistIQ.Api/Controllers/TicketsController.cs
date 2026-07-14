using AssistIQ.Api.Auth;
using AssistIQ.Application.Common;
using AssistIQ.Application.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize(Policy = AuthorizationPolicies.TicketsManage)]
public sealed class TicketsController(TicketService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.CreateAsync(request, cancellationToken));
        }
        catch (AppException exception)
        {
            return ToErrorResult(exception);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketSummaryDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await service.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.GetAsync(id, cancellationToken));
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
