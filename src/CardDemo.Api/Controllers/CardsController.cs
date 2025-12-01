using CardDemo.Application.Features.Cards.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CardsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("account/{accountId}")]
    public async Task<IActionResult> GetCardsByAccount(long accountId)
    {
        var query = new GetCardsByAccountQuery(accountId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}
