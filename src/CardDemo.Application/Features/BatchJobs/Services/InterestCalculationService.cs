using CardDemo.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CardDemo.Application.Features.BatchJobs.Services;

/// <summary>
/// Interest Calculation Service - Equivalent to CBACT02C COBOL program
/// Calculates and applies interest charges to accounts
/// </summary>
public class InterestCalculationService
{
    private readonly ICardDemoDbContext _dbContext;
    private readonly ILogger<InterestCalculationService> _logger;
    
    // Default APR (Annual Percentage Rate) - typically would come from configuration
    private const decimal DefaultAPR = 0.1999m; // 19.99%
    private const decimal DaysInYear = 365m;

    public InterestCalculationService(
        ICardDemoDbContext dbContext,
        ILogger<InterestCalculationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Calculate and apply daily interest to all accounts with balances
    /// </summary>
    public async Task<BatchJobResult> CalculateInterestAsync(
        DateTime? calculationDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = BatchJobResult.Started("INTEREST-CALCULATION");
        var calcDate = calculationDate ?? DateTime.UtcNow.Date;
        
        _logger.LogInformation("Starting interest calculation job {JobId} for date {Date}", 
            result.JobId, calcDate);

        try
        {
            // Get all active accounts with positive balances
            var accounts = await _dbContext.Accounts
                .Where(a => a.ActiveStatus == "Y" && a.CurrentBalance > 0)
                .ToListAsync(cancellationToken);

            result.RecordsProcessed = accounts.Count;
            _logger.LogInformation("Found {Count} accounts with balances for interest calculation", accounts.Count);

            foreach (var account in accounts)
            {
                try
                {
                    // Calculate daily interest rate
                    var dailyRate = DefaultAPR / DaysInYear;
                    
                    // Calculate interest (simple daily interest)
                    var dailyInterest = Math.Round(account.CurrentBalance * dailyRate, 2);
                    
                    if (dailyInterest > 0)
                    {
                        // Create interest transaction
                        var interestTransaction = new Domain.Entities.Transaction
                        {
                            TransactionId = GenerateTransactionId(),
                            AccountId = account.AccountId,
                            CardNumber = "SYSTEM-INTEREST",
                            TransactionType = "IN", // Interest
                            CategoryCode = 9999, // System category
                            TransactionSource = "BATCH",
                            Description = $"DAILY INTEREST CHARGE - {calcDate:yyyy-MM-dd}",
                            Amount = dailyInterest,
                            MerchantName = "CARDDEMO INTEREST",
                            TransactionDate = calcDate,
                            ProcessedFlag = "Y" // Already processed
                        };

                        // Update account balance
                        account.CurrentBalance += dailyInterest;
                        account.CurrentCycleDebit += dailyInterest;

                        _dbContext.Transactions.Add(interestTransaction);

                        _logger.LogDebug("Applied interest {Interest:C} to account {AccountId}. New balance: {Balance:C}",
                            dailyInterest, account.AccountId, account.CurrentBalance);
                    }

                    result.RecordsSucceeded++;
                }
                catch (Exception ex)
                {
                    result.RecordsFailed++;
                    result.Errors.Add($"Account {account.AccountId}: {ex.Message}");
                    _logger.LogError(ex, "Error calculating interest for account {AccountId}", account.AccountId);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            result.Complete();

            _logger.LogInformation("Interest calculation completed. Processed: {Processed}, Succeeded: {Succeeded}, Failed: {Failed}",
                result.RecordsProcessed, result.RecordsSucceeded, result.RecordsFailed);
        }
        catch (Exception ex)
        {
            result.Fail(ex.Message);
            _logger.LogError(ex, "Interest calculation job failed");
        }

        return result;
    }

    private static string GenerateTransactionId()
    {
        return $"INT{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
    }
}
