using AssistIQ.Api.Auth;
using AssistIQ.Api.Security;
using AssistIQ.Application.Drafts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.DraftsManage)]
public sealed class DraftsController(DraftService service) : ControllerBase
{
    [HttpPost("api/tickets/{ticketId:guid}/drafts/generate")]
    [EnableRateLimiting(ApiRateLimitPolicies.AiDraft)]
    public async Task<ActionResult<DraftDto>> Generate(
        Guid ticketId,
        GenerateDraftRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await service.GenerateAsync(ticketId, request, cancellationToken));
    }

    [HttpPatch("api/drafts/{id:guid}")]
    public async Task<ActionResult<DraftDto>> Update(
        Guid id,
        UpdateDraftRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await service.UpdateAsync(id, request, cancellationToken));
    }

    [HttpPost("api/drafts/{id:guid}/send")]
    public async Task<ActionResult<DraftDto>> Send(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await service.SendAsync(id, cancellationToken));
    }
}
