using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Accounts.Queries;

public record GetAllAccountsQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<AccountDto>>;

public record AccountDto
{
    public long AccountId { get; init; }
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = default!;
    public string ActiveStatus { get; init; } = default!;
    public decimal CurrentBalance { get; init; }
    public decimal CreditLimit { get; init; }
    public decimal CashCreditLimit { get; init; }
    public decimal AvailableCredit { get; init; }
    public int NumberOfCards { get; init; }
    public DateTime OpenDate { get; init; }
    public DateTime ExpirationDate { get; init; }
    public string GroupId { get; init; } = default!;
    public bool IsActive => ActiveStatus == "Y";
}

public class GetAllAccountsQueryHandler : IRequestHandler<GetAllAccountsQuery, PagedResult<AccountDto>>
{
    private readonly ICardDemoDbContext _context;

    public GetAllAccountsQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Accounts
            .Include(a => a.Customer)
            .Include(a => a.Cards)
            .OrderBy(a => a.AccountId);

        var totalCount = await query.CountAsync(cancellationToken);

        var accounts = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AccountDto
            {
                AccountId = a.AccountId,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer.FirstName + " " + a.Customer.LastName,
                ActiveStatus = a.ActiveStatus,
                CurrentBalance = a.CurrentBalance,
                CreditLimit = a.CreditLimit,
                CashCreditLimit = a.CashCreditLimit,
                AvailableCredit = a.CreditLimit - a.CurrentBalance,
                NumberOfCards = a.Cards.Count,
                OpenDate = a.OpenDate,
                ExpirationDate = a.ExpirationDate,
                GroupId = a.GroupId
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AccountDto>
        {
            Items = accounts,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
