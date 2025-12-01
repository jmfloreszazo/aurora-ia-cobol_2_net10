using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Users.Queries;

public record GetUserByIdQuery(string UserId) : IRequest<UserDto?>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly ICardDemoDbContext _context;

    public GetUserByIdQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Where(u => u.UserId == request.UserId)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                UserType = u.UserType.ToString(),
                Status = u.IsActive ? "A" : "I",
                IsActive = u.IsActive,
                IsLocked = u.IsLocked,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }
}
