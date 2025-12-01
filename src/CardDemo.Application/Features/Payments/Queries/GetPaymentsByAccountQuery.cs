using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Payments.Queries;

public record GetPaymentsByAccountQuery(
    long AccountId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<PaymentDto>>;

public class GetPaymentsByAccountQueryHandler : IRequestHandler<GetPaymentsByAccountQuery, PagedResult<PaymentDto>>
{
    private readonly ICardDemoDbContext _context;

    public GetPaymentsByAccountQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PaymentDto>> Handle(GetPaymentsByAccountQuery request, CancellationToken cancellationToken)
    {
        // Get payment transactions (type "02" = Bill Payment)
        var query = _context.Transactions
            .Where(t => t.AccountId == request.AccountId && t.TransactionType == "02")
            .OrderByDescending(t => t.TransactionDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var payments = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new PaymentDto
            {
                ConfirmationNumber = $"PAY{t.TransactionDate:yyyyMMddHHmmss}{t.AccountId:D11}",
                Timestamp = t.TransactionDate,
                AccountId = t.AccountId,
                Amount = t.Amount,
                NewBalance = 0, // Historical balance not stored
                TransactionId = t.TransactionId
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PaymentDto>
        {
            Items = payments,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
