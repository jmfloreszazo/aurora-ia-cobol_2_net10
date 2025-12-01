using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Interfaces;
using CardDemo.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Transactions.Commands;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionResponse>
{
    private readonly ICardDemoDbContext _context;

    public CreateTransactionCommandHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionResponse> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        // Verify account exists
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountId == request.AccountId, cancellationToken);

        if (account == null)
        {
            throw new KeyNotFoundException($"Account {request.AccountId} not found");
        }

        // Verify card exists and belongs to account
        var card = await _context.Cards
            .FirstOrDefaultAsync(c => c.CardNumber == request.CardNumber && c.AccountId == request.AccountId, cancellationToken);

        if (card == null)
        {
            throw new KeyNotFoundException($"Card {request.CardNumber} not found for account {request.AccountId}");
        }

        // Verify transaction type exists
        var transactionType = await _context.TransactionTypes
            .Include(tt => tt.Category)
            .FirstOrDefaultAsync(tt => tt.TypeCode == request.TransactionType, cancellationToken);

        if (transactionType == null)
        {
            throw new KeyNotFoundException($"Transaction type {request.TransactionType} not found");
        }

        // Generate transaction ID (simplified - in production use more robust generation)
        var transactionId = $"T{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        var transaction = new Transaction
        {
            TransactionId = transactionId,
            AccountId = request.AccountId,
            CardNumber = request.CardNumber,
            TransactionType = request.TransactionType,
            CategoryCode = request.CategoryCode,
            TransactionSource = request.TransactionSource,
            Description = request.Description,
            Amount = request.Amount,
            MerchantId = request.MerchantId,
            MerchantName = request.MerchantName,
            MerchantCity = request.MerchantCity,
            MerchantZip = request.MerchantZip,
            TransactionDate = request.TransactionDate ?? DateTime.UtcNow,
            ProcessedFlag = "N",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "SYSTEM"
        };

        _context.Transactions.Add(transaction);

        // Update account balance
        account.CurrentBalance += request.Amount;
        if (request.Amount > 0)
        {
            account.CurrentCycleCredit += request.Amount;
        }
        else
        {
            account.CurrentCycleDebit += Math.Abs(request.Amount);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new TransactionResponse(
            transaction.TransactionId,
            transaction.AccountId,
            transaction.CardNumber,
            card.MaskedCardNumber,
            transaction.TransactionType,
            transactionType.TypeDescription,
            transaction.CategoryCode,
            transactionType.Category.CategoryDescription,
            transaction.TransactionSource,
            transaction.Description,
            transaction.Amount,
            transaction.MerchantId,
            transaction.MerchantName,
            transaction.MerchantCity,
            transaction.MerchantZip,
            transaction.OrigTransactionId,
            transaction.TransactionDate,
            transaction.IsProcessed
        );
    }
}
