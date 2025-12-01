using CardDemo.Application.Common.Interfaces;
using CardDemo.Domain.Entities;
using CardDemo.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Users.Commands;

public record CreateUserCommand(
    string UserId,
    string FirstName,
    string LastName,
    string UserType,
    string Password) : IRequest<UserDto>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly ICardDemoDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(ICardDemoDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException($"User {request.UserId} already exists");
        }

        // Parse user type
        if (!Enum.TryParse<UserRole>(request.UserType, true, out var userRole))
        {
            userRole = UserRole.USER;
        }

        // Create new user
        var user = new User
        {
            UserId = request.UserId.ToUpperInvariant(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserType = userRole,
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 0
        };
        user.PasswordHash = _passwordHasher.HashPassword(request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new UserDto
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            UserType = user.UserType.ToString(),
            Status = user.IsActive ? "A" : "I",
            IsActive = user.IsActive,
            IsLocked = user.IsLocked,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
