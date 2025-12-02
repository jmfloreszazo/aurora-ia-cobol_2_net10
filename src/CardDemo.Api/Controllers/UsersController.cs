using CardDemo.Application.Features.Users;
using CardDemo.Application.Features.Users.Commands;
using CardDemo.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardDemo.Api.Controllers;

/// <summary>
/// Controller for user administration (equivalent to COUSR00C-COUSR03C COBOL programs)
/// Supports User List, Add, Edit, and Delete operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all users with pagination (equivalent to COUSR00C - User List)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Retrieving all users, page {PageNumber}", pageNumber);
        
        var query = new GetAllUsersQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserById(string userId)
    {
        _logger.LogInformation("Retrieving user {UserId}", userId);
        
        var query = new GetUserByIdQuery(userId);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(new { message = $"User {userId} not found" });
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new user (equivalent to COUSR01C - Add User)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            _logger.LogInformation("Creating user {UserId}", request.UserId);
            
            var command = new CreateUserCommand(
                request.UserId,
                request.FirstName,
                request.LastName,
                request.UserType,
                request.Password
            );
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("User {UserId} created successfully", result.UserId);
            
            return CreatedAtAction(nameof(GetUserById), new { userId = result.UserId }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create user: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing user (equivalent to COUSR02C - Edit User)
    /// </summary>
    [HttpPut("{userId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
    {
        try
        {
            _logger.LogInformation("Updating user {UserId}", userId);
            
            var command = new UpdateUserCommand(
                userId,
                request.FirstName,
                request.LastName,
                request.UserType,
                request.Status,
                request.NewPassword
            );
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("User {UserId} updated successfully", userId);
            
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete (deactivate) a user (equivalent to COUSR03C - Delete User)
    /// </summary>
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        try
        {
            // Get the current user ID from claims
            var currentUserId = User.FindFirst("userId")?.Value ?? 
                                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            // Prevent users from deleting themselves
            if (string.Equals(currentUserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("User {UserId} attempted to delete themselves", userId);
                return BadRequest(new { message = "Cannot delete currently logged in user" });
            }

            _logger.LogInformation("Deleting user {UserId}", userId);
            
            var command = new DeleteUserCommand(userId);
            await _mediator.Send(command);
            
            _logger.LogInformation("User {UserId} deleted (deactivated) successfully", userId);
            
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
    }
}
