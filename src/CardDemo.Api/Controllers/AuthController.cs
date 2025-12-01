using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Login to the system
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var command = new LoginCommand(request.UserId, request.Password);
            var response = await _mediator.Send(command);
            
            _logger.LogInformation("User {UserId} logged in successfully", request.UserId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Failed login attempt for user {UserId}: {Message}", request.UserId, ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterCommand(
                request.UserId,
                request.Password,
                request.FirstName,
                request.LastName,
                request.UserType
            );
            
            var response = await _mediator.Send(command);
            
            _logger.LogInformation("New user {UserId} registered successfully", request.UserId);
            return CreatedAtAction(nameof(Register), new { userId = response.UserId }, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to register user {UserId}: {Message}", request.UserId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }
}
