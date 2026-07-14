using AssistIQ.Api.Auth;
using AssistIQ.Application.UsageLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Controllers;

[ApiController]
[Route("api/usage-logs")]
[Authorize(Policy = AuthorizationPolicies.UsageLogsView)]
public sealed class UsageLogsController(UsageLogQueryService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsageLogDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await service.ListAsync(cancellationToken));
    }
}
