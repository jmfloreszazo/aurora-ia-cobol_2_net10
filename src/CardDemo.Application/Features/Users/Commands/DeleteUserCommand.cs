using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Users.Commands;

public record DeleteUserCommand(string UserId) : IRequest<bool>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly ICardDemoDbContext _context;

    public DeleteUserCommandHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User {request.UserId} not found");
        }

        // Instead of hard delete, we deactivate the user
        user.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
