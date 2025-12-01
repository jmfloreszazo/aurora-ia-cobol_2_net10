using CardDemo.Application.Common.Interfaces;
using CardDemo.Domain.Entities;
using CardDemo.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Users.Commands;

public record UpdateUserCommand(
    string UserId,
    string? FirstName,
    string? LastName,
    string? UserType,
    string? Status,
    string? NewPassword) : IRequest<UserDto>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly ICardDemoDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateUserCommandHandler(ICardDemoDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User {request.UserId} not found");
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrEmpty(request.LastName))
        {
            user.LastName = request.LastName;
        }

        if (!string.IsNullOrEmpty(request.UserType) && Enum.TryParse<UserRole>(request.UserType, true, out var userRole))
        {
            user.UserType = userRole;
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            user.IsActive = request.Status.ToUpperInvariant() == "A";
        }

        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        }

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
