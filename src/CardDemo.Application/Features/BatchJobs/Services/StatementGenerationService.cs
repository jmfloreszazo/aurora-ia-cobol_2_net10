using CardDemo.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CardDemo.Application.Features.BatchJobs.Services;

/// <summary>
/// Statement Generation Service - Equivalent to CBSTM03A/CBSTM03B COBOL programs
/// Generates monthly account statements
/// </summary>
public class StatementGenerationService
{
    private readonly ICardDemoDbContext _dbContext;
    private readonly ILogger<StatementGenerationService> _logger;

    public StatementGenerationService(
        ICardDemoDbContext dbContext,
        ILogger<StatementGenerationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Generate statements for all active accounts
    /// </summary>
    public async Task<BatchJobResult> GenerateStatementsAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var result = BatchJobResult.Started("STATEMENT-GENERATION");
        var statementDate = new DateTime(year, month, 1);
        var endDate = statementDate.AddMonths(1).AddDays(-1);
        
        _logger.LogInformation("Starting statement generation job {JobId} for {Year}-{Month:D2}", 
            result.JobId, year, month);

        try
        {
            var accounts = await _dbContext.Accounts
                .Where(a => a.ActiveStatus == "Y")
                .Include(a => a.Customer)
                .Include(a => a.Cards)
                .ToListAsync(cancellationToken);

            result.RecordsProcessed = accounts.Count;
            var statements = new List<AccountStatement>();

            foreach (var account in accounts)
            {
                try
                {
                    // Get transactions for the statement period
                    var transactions = await _dbContext.Transactions
                        .Where(t => t.AccountId == account.AccountId &&
                                    t.TransactionDate >= statementDate &&
                                    t.TransactionDate <= endDate &&
                                    t.ProcessedFlag == "Y")
                        .OrderBy(t => t.TransactionDate)
                        .ToListAsync(cancellationToken);

                    var statement = new AccountStatement
                    {
                        AccountId = account.AccountId,
                        CustomerName = account.Customer?.FullName ?? "Unknown",
                        StatementDate = endDate,
                        StatementPeriodStart = statementDate,
                        StatementPeriodEnd = endDate,
                        PreviousBalance = account.CurrentBalance - transactions.Sum(t => t.Amount),
                        TotalDebits = transactions.Where(t => t.Amount > 0).Sum(t => t.Amount),
                        TotalCredits = transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount)),
                        NewBalance = account.CurrentBalance,
                        MinimumPaymentDue = CalculateMinimumPayment(account.CurrentBalance),
                        PaymentDueDate = endDate.AddDays(25), // 25 days grace period
                        CreditLimit = account.CreditLimit,
                        AvailableCredit = account.CreditLimit - account.CurrentBalance,
                        Transactions = transactions.Select(t => new StatementTransaction
                        {
                            Date = t.TransactionDate,
                            Description = t.Description,
                            Amount = t.Amount,
                            MerchantName = t.MerchantName ?? "",
                            Type = t.Amount > 0 ? "DEBIT" : "CREDIT"
                        }).ToList()
                    };

                    statements.Add(statement);
                    result.RecordsSucceeded++;

                    _logger.LogDebug("Generated statement for account {AccountId}: Balance {Balance:C}, {TxnCount} transactions",
                        account.AccountId, statement.NewBalance, transactions.Count);
                }
                catch (Exception ex)
                {
                    result.RecordsFailed++;
                    result.Errors.Add($"Account {account.AccountId}: {ex.Message}");
                    _logger.LogError(ex, "Error generating statement for account {AccountId}", account.AccountId);
                }
            }

            // Generate output file (similar to COBOL spool file)
            var outputPath = GenerateStatementsFile(statements, year, month);
            result.OutputFilePath = outputPath;
            
            result.Complete();

            _logger.LogInformation("Statement generation completed. Processed: {Processed}, Succeeded: {Succeeded}, Failed: {Failed}. Output: {OutputPath}",
                result.RecordsProcessed, result.RecordsSucceeded, result.RecordsFailed, outputPath);
        }
        catch (Exception ex)
        {
            result.Fail(ex.Message);
            _logger.LogError(ex, "Statement generation job failed");
        }

        return result;
    }

    private decimal CalculateMinimumPayment(decimal balance)
    {
        if (balance <= 0) return 0;
        
        // Minimum payment is the greater of $25 or 2% of balance
        var percentPayment = balance * 0.02m;
        return Math.Max(25m, Math.Round(percentPayment, 2));
    }

    private string GenerateStatementsFile(List<AccountStatement> statements, int year, int month)
    {
        var sb = new StringBuilder();
        
        // Header (similar to COBOL report header)
        sb.AppendLine("================================================================================");
        sb.AppendLine($"                     CARDDEMO MONTHLY STATEMENTS - {year}-{month:D2}");
        sb.AppendLine($"                     GENERATED: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("================================================================================");
        sb.AppendLine();

        foreach (var statement in statements)
        {
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine($"ACCOUNT: {statement.AccountId,-15}  CUSTOMER: {statement.CustomerName}");
            sb.AppendLine($"STATEMENT DATE: {statement.StatementDate:yyyy-MM-dd}");
            sb.AppendLine($"PERIOD: {statement.StatementPeriodStart:yyyy-MM-dd} TO {statement.StatementPeriodEnd:yyyy-MM-dd}");
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine();
            sb.AppendLine($"  PREVIOUS BALANCE:           {statement.PreviousBalance,15:C}");
            sb.AppendLine($"  TOTAL DEBITS:               {statement.TotalDebits,15:C}");
            sb.AppendLine($"  TOTAL CREDITS:              {statement.TotalCredits,15:C}");
            sb.AppendLine($"  NEW BALANCE:                {statement.NewBalance,15:C}");
            sb.AppendLine();
            sb.AppendLine($"  CREDIT LIMIT:               {statement.CreditLimit,15:C}");
            sb.AppendLine($"  AVAILABLE CREDIT:           {statement.AvailableCredit,15:C}");
            sb.AppendLine();
            sb.AppendLine($"  MINIMUM PAYMENT DUE:        {statement.MinimumPaymentDue,15:C}");
            sb.AppendLine($"  PAYMENT DUE DATE:           {statement.PaymentDueDate:yyyy-MM-dd}");
            sb.AppendLine();

            if (statement.Transactions.Any())
            {
                sb.AppendLine("  TRANSACTIONS:");
                sb.AppendLine("  DATE        DESCRIPTION                                    TYPE     AMOUNT");
                sb.AppendLine("  ----------  ---------------------------------------------  ------  ----------");
                
                foreach (var txn in statement.Transactions)
                {
                    var desc = txn.Description.Length > 45 ? txn.Description[..45] : txn.Description;
                    sb.AppendLine($"  {txn.Date:yyyy-MM-dd}  {desc,-45}  {txn.Type,-6}  {txn.Amount,10:C}");
                }
            }
            else
            {
                sb.AppendLine("  NO TRANSACTIONS THIS PERIOD");
            }
            
            sb.AppendLine();
        }

        // Footer
        sb.AppendLine("================================================================================");
        sb.AppendLine($"                     TOTAL STATEMENTS GENERATED: {statements.Count}");
        sb.AppendLine("================================================================================");

        // In a real implementation, this would write to a file system or blob storage
        // For now, we'll return a virtual path
        return $"statements/STMT_{year}{month:D2}_{DateTime.UtcNow:yyyyMMddHHmmss}.txt";
    }
}

public class AccountStatement
{
    public long AccountId { get; set; }
    public string CustomerName { get; set; } = default!;
    public DateTime StatementDate { get; set; }
    public DateTime StatementPeriodStart { get; set; }
    public DateTime StatementPeriodEnd { get; set; }
    public decimal PreviousBalance { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public decimal NewBalance { get; set; }
    public decimal MinimumPaymentDue { get; set; }
    public DateTime PaymentDueDate { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit { get; set; }
    public List<StatementTransaction> Transactions { get; set; } = new();
}

public class StatementTransaction
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = default!;
    public decimal Amount { get; set; }
    public string MerchantName { get; set; } = default!;
    public string Type { get; set; } = default!;
}
