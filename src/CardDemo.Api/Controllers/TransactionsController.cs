using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Features.Transactions.Commands;
using CardDemo.Application.Features.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(IMediator mediator, ILogger<TransactionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new transaction
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        try
        {
            var command = new CreateTransactionCommand(
                request.AccountId,
                request.CardNumber,
                request.TransactionType,
                request.CategoryCode,
                request.TransactionSource,
                request.Description,
                request.Amount,
                request.MerchantId,
                request.MerchantName,
                request.MerchantCity,
                request.MerchantZip,
                request.TransactionDate
            );
            
            var response = await _mediator.Send(command);
            
            _logger.LogInformation("Transaction {TransactionId} created for account {AccountId}", 
                response.TransactionId, request.AccountId);
            
            return CreatedAtAction(nameof(CreateTransaction), new { id = response.TransactionId }, response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Failed to create transaction: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid transaction request: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get transactions by account ID with pagination
    /// </summary>
    [HttpGet("account/{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactionsByAccount(
        long accountId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetTransactionsByAccountQuery(accountId, pageNumber, pageSize);
        var response = await _mediator.Send(query);
        
        return Ok(response);
    }

    /// <summary>
    /// Get transactions by card number with pagination
    /// </summary>
    [HttpGet("card/{cardNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactionsByCard(
        string cardNumber,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetTransactionsByCardQuery(cardNumber, pageNumber, pageSize);
        var response = await _mediator.Send(query);
        
        return Ok(response);
    }
}
