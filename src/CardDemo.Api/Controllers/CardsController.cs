using CardDemo.Application.Features.Cards.Commands;
using CardDemo.Application.Features.Cards.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardDemo.Api.Controllers;

/// <summary>
/// Controller for card operations (equivalent to COCRDLIC, COCRDSLC, COCRDUPC COBOL programs)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CardsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CardsController> _logger;

    public CardsController(IMediator mediator, ILogger<CardsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all cards with pagination (equivalent to COCRDLIC - Card List)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCards(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAllCardsQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    /// <summary>
    /// Get card by card number (equivalent to COCRDSLC - Card Detail/Select)
    /// </summary>
    [HttpGet("{cardNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCardByNumber(string cardNumber)
    {
        var query = new GetCardByNumberQuery(cardNumber);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(new { message = $"Card {cardNumber} not found" });
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Get cards by account ID
    /// </summary>
    [HttpGet("account/{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCardsByAccount(long accountId)
    {
        var query = new GetCardsByAccountQuery(accountId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Update card (equivalent to COCRDUPC - Card Update)
    /// </summary>
    [HttpPut("{cardNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCard(string cardNumber, [FromBody] UpdateCardRequest request)
    {
        try
        {
            _logger.LogInformation("Updating card {CardNumber}", cardNumber);
            
            var command = new UpdateCardCommand(
                cardNumber,
                request.CardType,
                request.EmbossedName,
                request.ExpirationDate,
                request.ActiveStatus
            );
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Card {CardNumber} updated successfully", cardNumber);
            
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Card not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
    }
}

public record UpdateCardRequest
{
    public string? CardType { get; init; }
    public string? EmbossedName { get; init; }
    public string? ExpirationDate { get; init; }
    public string? ActiveStatus { get; init; }
}
