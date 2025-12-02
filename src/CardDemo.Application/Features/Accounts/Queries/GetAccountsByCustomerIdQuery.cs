using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Accounts.Queries;

public record GetAccountsByCustomerIdQuery(int CustomerId) : IRequest<List<AccountDto>>;

public class GetAccountsByCustomerIdQueryHandler : IRequestHandler<GetAccountsByCustomerIdQuery, List<AccountDto>>
{
    private readonly ICardDemoDbContext _context;

    public GetAccountsByCustomerIdQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<List<AccountDto>> Handle(GetAccountsByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _context.Accounts
            .Include(a => a.Customer)
            .Include(a => a.Cards)
            .Where(a => a.CustomerId == request.CustomerId)
            .Select(a => new AccountDto
            {
                AccountId = a.AccountId,
                CustomerId = a.CustomerId,
                CustomerName = $"{a.Customer.FirstName} {a.Customer.LastName}",
                ActiveStatus = a.ActiveStatus,
                CurrentBalance = a.CurrentBalance,
                CreditLimit = a.CreditLimit,
                CashCreditLimit = a.CashCreditLimit,
                OpenDate = a.OpenDate,
                ExpirationDate = a.ExpirationDate,
                GroupId = a.GroupId ?? string.Empty
            })
            .ToListAsync(cancellationToken);

        return accounts;
    }
}
