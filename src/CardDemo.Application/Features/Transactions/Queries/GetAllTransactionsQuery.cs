using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Transactions.Queries;

public record GetAllTransactionsQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<TransactionDto>>;

public class GetAllTransactionsQueryHandler : IRequestHandler<GetAllTransactionsQuery, PagedResult<TransactionDto>>
{
    private readonly ICardDemoDbContext _context;

    public GetAllTransactionsQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TransactionDto>> Handle(GetAllTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.TransactionTypeNavigation)
            .OrderByDescending(t => t.TransactionDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TransactionDto
            {
                TransactionId = t.TransactionId,
                AccountId = t.AccountId,
                CardNumber = t.CardNumber,
                MaskedCardNumber = t.CardNumber.Length >= 4 
                    ? $"**** **** **** {t.CardNumber.Substring(t.CardNumber.Length - 4)}" 
                    : t.CardNumber,
                TransactionType = t.TransactionType,
                TransactionTypeDescription = t.TransactionTypeNavigation.TypeDescription,
                CategoryCode = t.CategoryCode,
                CategoryDescription = t.Category.CategoryDescription,
                TransactionSource = t.TransactionSource,
                Description = t.Description,
                Amount = t.Amount,
                MerchantId = t.MerchantId,
                MerchantName = t.MerchantName,
                MerchantCity = t.MerchantCity,
                TransactionDate = t.TransactionDate,
                ProcessedFlag = t.ProcessedFlag,
                IsProcessed = t.ProcessedFlag == "Y"
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<TransactionDto>
        {
            Items = transactions,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
