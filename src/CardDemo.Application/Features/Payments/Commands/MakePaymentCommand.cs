using CardDemo.Application.Common.Interfaces;
using CardDemo.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Payments.Commands;

public record MakePaymentCommand(
    long AccountId,
    decimal Amount,
    DateTime? PaymentDate = null) : IRequest<PaymentDto>;

public class MakePaymentCommandHandler : IRequestHandler<MakePaymentCommand, PaymentDto>
{
    private readonly ICardDemoDbContext _context;

    public MakePaymentCommandHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto> Handle(MakePaymentCommand request, CancellationToken cancellationToken)
    {
        // Get the account
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountId == request.AccountId, cancellationToken);

        if (account == null)
        {
            throw new KeyNotFoundException($"Account {request.AccountId} not found");
        }

        // Get a card associated with this account for the transaction
        var card = await _context.Cards
            .FirstOrDefaultAsync(c => c.AccountId == request.AccountId, cancellationToken);

        if (card == null)
        {
            throw new InvalidOperationException($"No card found for account {request.AccountId}");
        }

        // Get the current balance - if requesting to pay more than balance, pay the full balance
        var paymentAmount = request.Amount > 0 ? request.Amount : account.CurrentBalance;
        if (paymentAmount > account.CurrentBalance)
        {
            paymentAmount = account.CurrentBalance;
        }

        if (account.CurrentBalance <= 0)
        {
            throw new InvalidOperationException("Account has no balance to pay");
        }

        // Generate transaction ID (similar to COBOL logic in COBIL00C)
        var lastTransaction = await _context.Transactions
            .OrderByDescending(t => t.TransactionId)
            .FirstOrDefaultAsync(cancellationToken);

        var newTransactionId = "0000000000000001";
        if (lastTransaction != null && long.TryParse(lastTransaction.TransactionId, out var lastId))
        {
            newTransactionId = (lastId + 1).ToString("D16");
        }

        var paymentDate = request.PaymentDate ?? DateTime.UtcNow;

        // Create payment transaction (equivalent to COBIL00C WRITE-TRANSACT-FILE)
        var transaction = new Transaction
        {
            TransactionId = newTransactionId,
            AccountId = request.AccountId,
            CardNumber = card.CardNumber,
            TransactionType = "02", // Bill Payment type
            CategoryCode = 2, // Bill Payment category
            TransactionSource = "POS TERM",
            Description = "BILL PAYMENT - ONLINE",
            Amount = paymentAmount,
            MerchantId = "999999999",
            MerchantName = "BILL PAYMENT",
            MerchantCity = "N/A",
            MerchantZip = "N/A",
            TransactionDate = paymentDate,
            ProcessedFlag = "Y"
        };

        _context.Transactions.Add(transaction);

        // Update account balance (equivalent to COBIL00C UPDATE-ACCTDAT-FILE)
        var previousBalance = account.CurrentBalance;
        account.CurrentBalance -= paymentAmount;

        await _context.SaveChangesAsync(cancellationToken);

        // Generate confirmation number
        var confirmationNumber = $"PAY{paymentDate:yyyyMMddHHmmss}{request.AccountId:D11}";

        return new PaymentDto
        {
            ConfirmationNumber = confirmationNumber,
            Timestamp = paymentDate,
            AccountId = request.AccountId,
            Amount = paymentAmount,
            NewBalance = account.CurrentBalance,
            TransactionId = newTransactionId
        };
    }
}
