using AssistIQ.Api.Auth;
using AssistIQ.Application.AuditLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Policy = AuthorizationPolicies.AuditLogsView)]
public sealed class AuditLogsController(AuditLogQueryService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await service.ListAsync(cancellationToken));
    }
}
