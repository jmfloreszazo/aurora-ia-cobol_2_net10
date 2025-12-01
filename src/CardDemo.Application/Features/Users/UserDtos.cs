using CardDemo.Domain.Enums;

namespace CardDemo.Application.Features.Users;

public record UserDto
{
    public string UserId { get; init; } = default!;
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string UserType { get; init; } = default!;
    public string Status { get; init; } = default!;
    public bool IsActive { get; init; }
    public bool IsLocked { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateUserRequest
{
    public string UserId { get; init; } = default!;
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string UserType { get; init; } = default!;
    public string Password { get; init; } = default!;
}

public record UpdateUserRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? UserType { get; init; }
    public string? Status { get; init; }
    public string? NewPassword { get; init; }
}
