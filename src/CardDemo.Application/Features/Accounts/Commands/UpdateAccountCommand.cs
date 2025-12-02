using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Accounts.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Accounts.Commands;

public record UpdateAccountCommand(
    long AccountId,
    string? ActiveStatus,
    decimal? CreditLimit,
    decimal? CashCreditLimit,
    DateTime? ExpirationDate,
    string? GroupId) : IRequest<AccountDto>;

public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, AccountDto>
{
    private readonly ICardDemoDbContext _context;

    public UpdateAccountCommandHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<AccountDto> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.AccountId == request.AccountId, cancellationToken);

        if (account == null)
        {
            throw new KeyNotFoundException($"Account {request.AccountId} not found");
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.ActiveStatus))
        {
            account.ActiveStatus = request.ActiveStatus;
        }

        if (request.CreditLimit.HasValue)
        {
            account.CreditLimit = request.CreditLimit.Value;
        }

        if (request.CashCreditLimit.HasValue)
        {
            account.CashCreditLimit = request.CashCreditLimit.Value;
        }

        if (request.ExpirationDate.HasValue)
        {
            account.ExpirationDate = request.ExpirationDate.Value;
        }

        if (!string.IsNullOrEmpty(request.GroupId))
        {
            account.GroupId = request.GroupId;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new AccountDto
        {
            AccountId = account.AccountId,
            CustomerId = account.CustomerId,
            CustomerName = (account.Customer != null ? $"{account.Customer.FirstName} {account.Customer.LastName}" : string.Empty)!,
            ActiveStatus = account.ActiveStatus,
            CurrentBalance = account.CurrentBalance,
            CreditLimit = account.CreditLimit,
            CashCreditLimit = account.CashCreditLimit,
            OpenDate = account.OpenDate,
            ExpirationDate = account.ExpirationDate,
            GroupId = account.GroupId
        };
    }
}
