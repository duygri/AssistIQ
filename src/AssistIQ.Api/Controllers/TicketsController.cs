using AssistIQ.Api.Auth;
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
    [Consumes("application/json")]
    public async Task<ActionResult<TicketDto>> Create(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        return Ok(await service.CreateAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketSummaryDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await service.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await service.GetAsync(id, cancellationToken));
    }
}
