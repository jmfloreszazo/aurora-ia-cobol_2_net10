using CardDemo.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace CardDemo.Application.Features.BatchJobs.Services;

/// <summary>
/// Data Export/Import Service - Equivalent to CBEXPORT/CBIMPORT COBOL programs
/// Exports and imports data in various formats
/// </summary>
public class DataExportImportService
{
    private readonly ICardDemoDbContext _dbContext;
    private readonly ILogger<DataExportImportService> _logger;

    public DataExportImportService(
        ICardDemoDbContext dbContext,
        ILogger<DataExportImportService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Export account data - equivalent to CBEXPORT for ACCTDAT
    /// </summary>
    public async Task<BatchJobResult> ExportAccountsAsync(
        ExportFormat format = ExportFormat.FixedWidth,
        CancellationToken cancellationToken = default)
    {
        var result = BatchJobResult.Started("EXPORT-ACCOUNTS");
        _logger.LogInformation("Starting account export job {JobId} in {Format} format", result.JobId, format);

        try
        {
            var accounts = await _dbContext.Accounts
                .Include(a => a.Customer)
                .OrderBy(a => a.AccountId)
                .ToListAsync(cancellationToken);

            result.RecordsProcessed = accounts.Count;

            var output = format switch
            {
                ExportFormat.FixedWidth => ExportAccountsFixedWidth(accounts),
                ExportFormat.Csv => ExportAccountsCsv(accounts),
                ExportFormat.Json => ExportAccountsJson(accounts),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };

            result.RecordsSucceeded = accounts.Count;
            result.OutputFilePath = $"exports/ACCTDAT_{DateTime.UtcNow:yyyyMMddHHmmss}.{GetExtension(format)}";
            result.Complete();

            _logger.LogInformation("Account export completed. {Count} records exported to {Path}",
                accounts.Count, result.OutputFilePath);
        }
        catch (Exception ex)
        {
            result.Fail(ex.Message);
            _logger.LogError(ex, "Account export job failed");
        }

        return result;
    }

    /// <summary>
    /// Export transactions - equivalent to CBEXPORT for TRANSACT
    /// </summary>
    public async Task<BatchJobResult> ExportTransactionsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        ExportFormat format = ExportFormat.FixedWidth,
        CancellationToken cancellationToken = default)
    {
        var result = BatchJobResult.Started("EXPORT-TRANSACTIONS");
        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;

        _logger.LogInformation("Starting transaction export job {JobId} from {From} to {To}", 
            result.JobId, from, to);

        try
        {
            var transactions = await _dbContext.Transactions
                .Where(t => t.TransactionDate >= from && t.TransactionDate <= to)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync(cancellationToken);

            result.RecordsProcessed = transactions.Count;

            var output = format switch
            {
                ExportFormat.FixedWidth => ExportTransactionsFixedWidth(transactions),
                ExportFormat.Csv => ExportTransactionsCsv(transactions),
                ExportFormat.Json => ExportTransactionsJson(transactions),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };

            result.RecordsSucceeded = transactions.Count;
            result.OutputFilePath = $"exports/TRANSACT_{from:yyyyMMdd}_{to:yyyyMMdd}.{GetExtension(format)}";
            result.Complete();

            _logger.LogInformation("Transaction export completed. {Count} records exported to {Path}",
                transactions.Count, result.OutputFilePath);
        }
        catch (Exception ex)
        {
            result.Fail(ex.Message);
            _logger.LogError(ex, "Transaction export job failed");
        }

        return result;
    }

    /// <summary>
    /// Export customers - equivalent to CBEXPORT for CUSTDAT
    /// </summary>
    public async Task<BatchJobResult> ExportCustomersAsync(
        ExportFormat format = ExportFormat.FixedWidth,
        CancellationToken cancellationToken = default)
    {
        var result = BatchJobResult.Started("EXPORT-CUSTOMERS");
        _logger.LogInformation("Starting customer export job {JobId}", result.JobId);

        try
        {
            var customers = await _dbContext.Customers
                .OrderBy(c => c.CustomerId)
                .ToListAsync(cancellationToken);

            result.RecordsProcessed = customers.Count;
            result.RecordsSucceeded = customers.Count;
            result.OutputFilePath = $"exports/CUSTDAT_{DateTime.UtcNow:yyyyMMddHHmmss}.{GetExtension(format)}";
            result.Complete();

            _logger.LogInformation("Customer export completed. {Count} records exported to {Path}",
                customers.Count, result.OutputFilePath);
        }
        catch (Exception ex)
        {
            result.Fail(ex.Message);
            _logger.LogError(ex, "Customer export job failed");
        }

        return result;
    }

    // Fixed-width format (like COBOL files)
    private string ExportAccountsFixedWidth(List<Domain.Entities.Account> accounts)
    {
        var sb = new StringBuilder();
        
        // Header record (COBOL style)
        sb.AppendLine($"HDR ACCTDAT {DateTime.UtcNow:yyyyMMddHHmmss} {accounts.Count:D10}");
        
        foreach (var a in accounts)
        {
            // Fixed-width record matching COBOL ACCOUNT-RECORD (300 bytes)
            sb.AppendLine(
                $"{a.AccountId:D11}" +           // ACCT-ID PIC 9(11)
                $"{a.ActiveStatus,-1}" +          // ACCT-ACTIVE-STATUS PIC X(01)
                $"{FormatDecimal(a.CurrentBalance, 10, 2)}" +  // ACCT-CURR-BAL PIC S9(10)V99
                $"{FormatDecimal(a.CreditLimit, 10, 2)}" +     // ACCT-CREDIT-LIMIT
                $"{FormatDecimal(a.CashCreditLimit, 10, 2)}" + // ACCT-CASH-CREDIT-LIMIT
                $"{a.OpenDate:yyyy-MM-dd}" +      // ACCT-OPEN-DATE PIC X(10)
                $"{a.ExpirationDate:yyyy-MM-dd}" + // ACCT-EXPIRATION-DATE
                $"{(a.ReissueDate?.ToString("yyyy-MM-dd") ?? "          ")}" + // ACCT-REISSUE-DATE
                $"{FormatDecimal(a.CurrentCycleCredit, 10, 2)}" +
                $"{FormatDecimal(a.CurrentCycleDebit, 10, 2)}" +
                $"{a.ZipCode,-10}" +              // ACCT-ADDR-ZIP
                $"{a.GroupId ?? "",-10}"          // ACCT-GROUP-ID
            );
        }
        
        // Trailer record
        sb.AppendLine($"TRL {accounts.Count:D10}");
        
        return sb.ToString();
    }

    private string ExportAccountsCsv(List<Domain.Entities.Account> accounts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("AccountId,ActiveStatus,CurrentBalance,CreditLimit,CashCreditLimit,OpenDate,ExpirationDate,ReissueDate,CurrentCycleCredit,CurrentCycleDebit,ZipCode,GroupId");
        
        foreach (var a in accounts)
        {
            sb.AppendLine($"{a.AccountId},{a.ActiveStatus},{a.CurrentBalance},{a.CreditLimit},{a.CashCreditLimit},{a.OpenDate:yyyy-MM-dd},{a.ExpirationDate:yyyy-MM-dd},{a.ReissueDate:yyyy-MM-dd},{a.CurrentCycleCredit},{a.CurrentCycleDebit},{a.ZipCode},{a.GroupId}");
        }
        
        return sb.ToString();
    }

    private string ExportAccountsJson(List<Domain.Entities.Account> accounts)
    {
        return JsonSerializer.Serialize(accounts.Select(a => new
        {
            a.AccountId,
            a.ActiveStatus,
            a.CurrentBalance,
            a.CreditLimit,
            a.CashCreditLimit,
            OpenDate = a.OpenDate.ToString("yyyy-MM-dd"),
            ExpirationDate = a.ExpirationDate.ToString("yyyy-MM-dd"),
            ReissueDate = a.ReissueDate?.ToString("yyyy-MM-dd"),
            a.CurrentCycleCredit,
            a.CurrentCycleDebit,
            a.ZipCode,
            a.GroupId
        }), new JsonSerializerOptions { WriteIndented = true });
    }

    private string ExportTransactionsFixedWidth(List<Domain.Entities.Transaction> transactions)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"HDR TRANSACT {DateTime.UtcNow:yyyyMMddHHmmss} {transactions.Count:D10}");
        
        foreach (var t in transactions)
        {
            sb.AppendLine(
                $"{t.TransactionId,-16}" +
                $"{t.TransactionType,-2}" +
                $"{t.CategoryCode:D4}" +
                $"{t.TransactionSource,-10}" +
                $"{t.Description,-100}" +
                $"{FormatDecimal(t.Amount, 9, 2)}" +
                $"{t.MerchantId ?? "",-9}" +
                $"{t.MerchantName ?? "",-50}" +
                $"{t.MerchantCity ?? "",-50}" +
                $"{t.MerchantZip ?? "",-10}" +
                $"{t.CardNumber,-16}" +
                $"{t.TransactionDate:yyyy-MM-dd HH:mm:ss}"
            );
        }
        
        sb.AppendLine($"TRL {transactions.Count:D10}");
        return sb.ToString();
    }

    private string ExportTransactionsCsv(List<Domain.Entities.Transaction> transactions)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TransactionId,TransactionType,CategoryCode,TransactionSource,Description,Amount,MerchantId,MerchantName,MerchantCity,MerchantZip,CardNumber,TransactionDate,ProcessedFlag");
        
        foreach (var t in transactions)
        {
            sb.AppendLine($"\"{t.TransactionId}\",\"{t.TransactionType}\",{t.CategoryCode},\"{t.TransactionSource}\",\"{t.Description}\",{t.Amount},\"{t.MerchantId}\",\"{t.MerchantName}\",\"{t.MerchantCity}\",\"{t.MerchantZip}\",\"{t.CardNumber}\",\"{t.TransactionDate:yyyy-MM-dd HH:mm:ss}\",\"{t.ProcessedFlag}\"");
        }
        
        return sb.ToString();
    }

    private string ExportTransactionsJson(List<Domain.Entities.Transaction> transactions)
    {
        return JsonSerializer.Serialize(transactions.Select(t => new
        {
            t.TransactionId,
            t.TransactionType,
            t.CategoryCode,
            t.TransactionSource,
            t.Description,
            t.Amount,
            t.MerchantId,
            t.MerchantName,
            t.MerchantCity,
            t.MerchantZip,
            t.CardNumber,
            TransactionDate = t.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss"),
            t.ProcessedFlag
        }), new JsonSerializerOptions { WriteIndented = true });
    }

    private static string FormatDecimal(decimal value, int intDigits, int decDigits)
    {
        var format = new string('0', intDigits) + "." + new string('0', decDigits);
        return value.ToString(format);
    }

    private static string GetExtension(ExportFormat format) => format switch
    {
        ExportFormat.FixedWidth => "dat",
        ExportFormat.Csv => "csv",
        ExportFormat.Json => "json",
        _ => "txt"
    };
}

public enum ExportFormat
{
    FixedWidth, // COBOL-style fixed width
    Csv,
    Json
}
