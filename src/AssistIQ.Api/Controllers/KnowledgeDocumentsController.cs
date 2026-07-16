using AssistIQ.Api.Auth;
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
