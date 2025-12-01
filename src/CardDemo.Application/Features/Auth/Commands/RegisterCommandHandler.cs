using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Interfaces;
using CardDemo.Domain.Entities;
using CardDemo.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly ICardDemoDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(ICardDemoDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException($"User {request.UserId} already exists");
        }

        // Parse user type
        if (!Enum.TryParse<UserRole>(request.UserType, out var userType))
        {
            throw new ArgumentException($"Invalid user type: {request.UserType}");
        }

        // Create new user
        var user = new User
        {
            UserId = request.UserId,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserType = userType,
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "SYSTEM"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(user.UserId, "User registered successfully");
    }
}
