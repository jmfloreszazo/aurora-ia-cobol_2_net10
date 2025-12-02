using CardDemo.Application.Features.Accounts.Commands;
using CardDemo.Application.Features.Accounts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardDemo.Api.Controllers;

/// <summary>
/// Controller for account operations (equivalent to COACTVWC and COACTUPC COBOL programs)
/// </summary>
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
    /// Get all accounts with pagination (equivalent to account list functionality)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAccounts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAllAccountsQuery(pageNumber, pageSize);
        var response = await _mediator.Send(query);
        
        return Ok(response);
    }

    /// <summary>
    /// Get account details by ID (equivalent to COACTVWC - Account View)
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
    /// Update account (equivalent to COACTUPC - Account Update)
    /// </summary>
    [HttpPut("{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAccount(long accountId, [FromBody] UpdateAccountRequest request)
    {
        try
        {
            _logger.LogInformation("Updating account {AccountId}", accountId);
            
            var command = new UpdateAccountCommand(
                accountId,
                request.ActiveStatus,
                request.CreditLimit,
                request.CashCreditLimit,
                request.ExpirationDate,
                request.GroupId
            );
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Account {AccountId} updated successfully", accountId);
            
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Account not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
    }
}

public record UpdateAccountRequest
{
    public string? ActiveStatus { get; init; }
    public decimal? CreditLimit { get; init; }
    public decimal? CashCreditLimit { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public string? GroupId { get; init; }
}
