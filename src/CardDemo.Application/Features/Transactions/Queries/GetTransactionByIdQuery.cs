using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Transactions.Queries;

public record GetTransactionByIdQuery(string TransactionId) : IRequest<TransactionDto?>;

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto?>
{
    private readonly ICardDemoDbContext _context;

    public GetTransactionByIdQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionDto?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.TransactionTypeNavigation)
            .Where(t => t.TransactionId == request.TransactionId)
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
            .FirstOrDefaultAsync(cancellationToken);

        return transaction;
    }
}
