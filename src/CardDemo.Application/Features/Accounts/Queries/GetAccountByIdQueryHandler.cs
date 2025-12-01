using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Accounts.Queries;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDetailResponse>
{
    private readonly ICardDemoDbContext _context;

    public GetAccountByIdQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<AccountDetailResponse> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .Include(a => a.Customer)
            .Include(a => a.Cards)
            .Include(a => a.Transactions.OrderByDescending(t => t.TransactionDate).Take(10))
            .FirstOrDefaultAsync(a => a.AccountId == request.AccountId, cancellationToken);

        if (account == null)
        {
            throw new KeyNotFoundException($"Account {request.AccountId} not found");
        }

        return new AccountDetailResponse(
            account.AccountId,
            account.CustomerId,
            account.Customer.FullName,
            account.ActiveStatus,
            account.CurrentBalance,
            account.CreditLimit,
            account.CashCreditLimit,
            account.AvailableCredit,
            account.CreditUtilization,
            account.OpenDate,
            account.ExpirationDate,
            account.ReissueDate,
            account.CurrentCycleCredit,
            account.CurrentCycleDebit,
            account.ZipCode,
            account.GroupId,
            account.Cards.Select(c => new CardSummaryResponse(
                c.CardNumber,
                c.MaskedCardNumber,
                c.CardType,
                c.ExpirationDate,
                c.ActiveStatus
            )).ToList(),
            account.Transactions.Select(t => new TransactionSummaryResponse(
                t.TransactionId,
                t.CardNumber,
                t.Card.MaskedCardNumber,
                t.Description,
                t.Amount,
                t.TransactionDate
            )).ToList()
        );
    }
}
