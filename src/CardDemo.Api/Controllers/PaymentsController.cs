using CardDemo.Application.Features.Payments;
using CardDemo.Application.Features.Payments.Commands;
using CardDemo.Application.Features.Payments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardDemo.Api.Controllers;

/// <summary>
/// Controller for bill payments (equivalent to COBIL00C COBOL program)
/// Handles online bill payments and payment history
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Make a bill payment
    /// </summary>
    /// <param name="request">Payment details</param>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MakePayment([FromBody] MakePaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Processing payment for account {AccountId}, amount {Amount}", 
                request.AccountId, request.Amount);
            
            var command = new MakePaymentCommand(
                request.AccountId,
                request.Amount,
                request.PaymentDate
            );
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Payment successful. Confirmation: {ConfirmationNumber}, Transaction: {TransactionId}", 
                result.ConfirmationNumber, result.TransactionId);
            
            return CreatedAtAction(nameof(MakePayment), new { id = result.ConfirmationNumber }, result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Payment failed: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Payment rejected: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get payment history for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    [HttpGet("account/{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentsByAccount(
        long accountId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetPaymentsByAccountQuery(accountId, pageNumber, pageSize);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
}
