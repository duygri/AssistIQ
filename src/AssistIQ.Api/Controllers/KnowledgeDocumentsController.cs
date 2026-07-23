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
    public async Task<ActionResult<PagedResult<KnowledgeDocumentDto>>> List(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        return Ok(await service.ListPagedAsync(pagination, cancellationToken));
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<ActionResult<KnowledgeDocumentDto>> Register(
        RegisterKnowledgeDocumentRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await service.RegisterAsync(request, cancellationToken));
    }

    [HttpPost("{id:guid}/disable")]
    public async Task<ActionResult<KnowledgeDocumentDto>> Disable(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await service.DisableAsync(id, cancellationToken));
    }
}

