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

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // En esta implementación simple, validamos que el token no esté vacío
        // En producción, validarías el refresh token contra una base de datos
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        // Por ahora retornamos error - implementación completa requeriría:
        // 1. Validar el refresh token
        // 2. Generar nuevo access token
        // 3. Generar nuevo refresh token
        // 4. Invalidar el refresh token anterior
        return Unauthorized(new { message = "Invalid refresh token" });
    }
}

public record RefreshTokenRequest(string RefreshToken);
