using CardDemo.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CardDemo.Application.Features.BatchJobs.Services;

/// <summary>
/// Transaction Posting Service - Equivalent to CBTRN01C and CBTRN02C COBOL programs
/// Processes daily transactions, validates cards/accounts, updates balances
/// </summary>
public class TransactionPostingService
{
    private readonly ICardDemoDbContext _dbContext;
    private readonly ILogger<TransactionPostingService> _logger;

    public TransactionPostingService(
        ICardDemoDbContext dbContext,
        ILogger<TransactionPostingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Post all unprocessed transactions - equivalent to CBTRN01C/CBTRN02C
    /// </summary>
    public async Task<BatchJobResult> PostTransactionsAsync(CancellationToken cancellationToken = default)
    {
        var result = BatchJobResult.Started("TRANSACTION-POSTING");
        _logger.LogInformation("Starting transaction posting job {JobId}", result.JobId);

        try
        {
            // Get all unprocessed transactions (ProcessedFlag = 'N')
            var unprocessedTransactions = await _dbContext.Transactions
                .Where(t => t.ProcessedFlag == "N")
                .Include(t => t.Account)
                .Include(t => t.Card)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync(cancellationToken);

            result.RecordsProcessed = unprocessedTransactions.Count;
            _logger.LogInformation("Found {Count} unprocessed transactions", unprocessedTransactions.Count);

            foreach (var transaction in unprocessedTransactions)
            {
                try
                {
                    // Validate card exists and is active (CBTRN02C validation)
                    var card = await _dbContext.Cards
                        .FirstOrDefaultAsync(c => c.CardNumber == transaction.CardNumber, cancellationToken);

                    if (card == null)
                    {
                        result.RecordsFailed++;
                        result.Errors.Add($"Transaction {transaction.TransactionId}: Card {transaction.CardNumber} not found");
                        continue;
                    }

                    if (card.ActiveStatus != "Y")
                    {
                        result.RecordsFailed++;
                        result.Errors.Add($"Transaction {transaction.TransactionId}: Card {transaction.CardNumber} is not active");
                        continue;
                    }

                    // Check card expiration
                    if (card.IsExpired)
                    {
                        result.RecordsFailed++;
                        result.Errors.Add($"Transaction {transaction.TransactionId}: Card {transaction.CardNumber} is expired");
                        continue;
                    }

                    // Validate account exists and is active
                    var account = await _dbContext.Accounts
                        .FirstOrDefaultAsync(a => a.AccountId == transaction.AccountId, cancellationToken);

                    if (account == null)
                    {
                        result.RecordsFailed++;
                        result.Errors.Add($"Transaction {transaction.TransactionId}: Account {transaction.AccountId} not found");
                        continue;
                    }

                    if (account.ActiveStatus != "Y")
                    {
                        result.RecordsFailed++;
                        result.Errors.Add($"Transaction {transaction.TransactionId}: Account {transaction.AccountId} is not active");
                        continue;
                    }

                    // Check credit limit for debits (purchases)
                    if (transaction.Amount > 0) // Positive = purchase/debit
                    {
                        var newBalance = account.CurrentBalance + transaction.Amount;
                        if (newBalance > account.CreditLimit)
                        {
                            result.RecordsFailed++;
                            result.Errors.Add($"Transaction {transaction.TransactionId}: Would exceed credit limit (Current: {account.CurrentBalance}, Amount: {transaction.Amount}, Limit: {account.CreditLimit})");
                            continue;
                        }
                    }

                    // Update account balance (CBTRN02C - WRITE-ACCTDAT-UPDATE)
                    account.CurrentBalance += transaction.Amount;
                    
                    // Update cycle totals
                    if (transaction.Amount > 0)
                    {
                        account.CurrentCycleDebit += transaction.Amount;
                    }
                    else
                    {
                        account.CurrentCycleCredit += Math.Abs(transaction.Amount);
                    }

                    // Mark transaction as processed
                    transaction.ProcessedFlag = "Y";

                    result.RecordsSucceeded++;
                    _logger.LogDebug("Posted transaction {TransactionId} - Amount: {Amount}, New Balance: {Balance}",
                        transaction.TransactionId, transaction.Amount, account.CurrentBalance);
                }
                catch (Exception ex)
                {
                    result.RecordsFailed++;
                    result.Errors.Add($"Transaction {transaction.TransactionId}: {ex.Message}");
                    _logger.LogError(ex, "Error processing transaction {TransactionId}", transaction.TransactionId);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            result.Complete();

            _logger.LogInformation("Transaction posting completed. Processed: {Processed}, Succeeded: {Succeeded}, Failed: {Failed}",
                result.RecordsProcessed, result.RecordsSucceeded, result.RecordsFailed);
        }
        catch (Exception ex)
        {
            result.Fail(ex.Message);
            _logger.LogError(ex, "Transaction posting job failed");
        }

        return result;
    }
}
