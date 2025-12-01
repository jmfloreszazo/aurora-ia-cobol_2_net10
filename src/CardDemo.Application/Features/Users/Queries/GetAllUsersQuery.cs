using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Users.Queries;

public record GetAllUsersQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<UserDto>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResult<UserDto>>
{
    private readonly ICardDemoDbContext _context;

    public GetAllUsersQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.OrderBy(u => u.UserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
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
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDto>
        {
            Items = users,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
