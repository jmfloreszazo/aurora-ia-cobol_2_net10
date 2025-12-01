using CardDemo.Application.Features.Customers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCustomers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAllCustomersQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(int id)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = "Customer not found" });

        return Ok(result);
    }
}
