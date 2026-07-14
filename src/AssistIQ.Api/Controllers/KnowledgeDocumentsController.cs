using AssistIQ.Api.Auth;
using AssistIQ.Application.Common;
using AssistIQ.Application.Knowledge;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Controllers;

[ApiController]
[Route("api/knowledge-documents")]
[Authorize(Policy = AuthorizationPolicies.KnowledgeManage)]
public sealed class KnowledgeDocumentsController(KnowledgeDocumentService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<KnowledgeDocumentDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await service.ListAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<KnowledgeDocumentDto>> Register(
        RegisterKnowledgeDocumentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.RegisterAsync(request, cancellationToken));
        }
        catch (AppException exception)
        {
            return ToErrorResult(exception);
        }
    }

    [HttpPost("{id:guid}/disable")]
    public async Task<ActionResult<KnowledgeDocumentDto>> Disable(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.DisableAsync(id, cancellationToken));
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
            StatusCodes.Status404NotFound => NotFound(body),
            StatusCodes.Status409Conflict => Conflict(body),
            _ => StatusCode(exception.StatusCode, body)
        };
    }
}
