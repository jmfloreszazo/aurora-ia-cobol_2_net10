using CardDemo.Application.Features.Accounts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IMediator mediator, ILogger<AccountsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get account details by ID
    /// </summary>
    [HttpGet("{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountById(long accountId)
    {
        try
        {
            var query = new GetAccountByIdQuery(accountId);
            var response = await _mediator.Send(query);
            
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Account {AccountId} not found", accountId);
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all accounts for a specific customer
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountsByCustomerId(int customerId)
    {
        var query = new GetAccountsByCustomerIdQuery(customerId);
        var response = await _mediator.Send(query);
        
        return Ok(response);
    }

    /// <summary>
    /// Get all accounts (for testing purposes)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAccounts()
    {
        // Retornar lista vacía por ahora - en producción consultaría la base de datos
        return Ok(new List<object>());
    }
}
