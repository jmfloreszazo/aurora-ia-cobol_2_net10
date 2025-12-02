using CardDemo.Application.Features.BatchJobs;
using CardDemo.Application.Features.BatchJobs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardDemo.Api.Controllers;

/// <summary>
/// Controller for batch job operations - Equivalent to COBOL JCL batch jobs
/// Provides API endpoints to trigger and monitor batch processing
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class BatchJobsController : ControllerBase
{
    private readonly TransactionPostingService _transactionPostingService;
    private readonly InterestCalculationService _interestCalculationService;
    private readonly StatementGenerationService _statementGenerationService;
    private readonly DataExportImportService _dataExportImportService;
    private readonly ILogger<BatchJobsController> _logger;

    // In-memory job history (in production, use a database or distributed cache)
    private static readonly List<BatchJobResult> _jobHistory = new();

    public BatchJobsController(
        TransactionPostingService transactionPostingService,
        InterestCalculationService interestCalculationService,
        StatementGenerationService statementGenerationService,
        DataExportImportService dataExportImportService,
        ILogger<BatchJobsController> logger)
    {
        _transactionPostingService = transactionPostingService;
        _interestCalculationService = interestCalculationService;
        _statementGenerationService = statementGenerationService;
        _dataExportImportService = dataExportImportService;
        _logger = logger;
    }

    /// <summary>
    /// Post all unprocessed transactions (CBTRN01C/CBTRN02C equivalent)
    /// </summary>
    [HttpPost("post-transactions")]
    [ProducesResponseType(typeof(BatchJobResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PostTransactions()
    {
        _logger.LogInformation("Transaction posting job triggered by user");
        
        var result = await _transactionPostingService.PostTransactionsAsync();
        _jobHistory.Add(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Calculate daily interest for all accounts (CBACT02C equivalent)
    /// </summary>
    [HttpPost("calculate-interest")]
    [ProducesResponseType(typeof(BatchJobResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CalculateInterest([FromQuery] DateTime? date = null)
    {
        _logger.LogInformation("Interest calculation job triggered for date {Date}", date ?? DateTime.UtcNow);
        
        var result = await _interestCalculationService.CalculateInterestAsync(date);
        _jobHistory.Add(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Generate monthly statements (CBSTM03A/CBSTM03B equivalent)
    /// </summary>
    [HttpPost("generate-statements")]
    [ProducesResponseType(typeof(BatchJobResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GenerateStatements(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;
        
        if (targetMonth < 1 || targetMonth > 12)
        {
            return BadRequest(new { message = "Month must be between 1 and 12" });
        }
        
        _logger.LogInformation("Statement generation job triggered for {Year}-{Month:D2}", targetYear, targetMonth);
        
        var result = await _statementGenerationService.GenerateStatementsAsync(targetYear, targetMonth);
        _jobHistory.Add(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Export accounts data (CBEXPORT equivalent for ACCTDAT)
    /// </summary>
    [HttpPost("export/accounts")]
    [ProducesResponseType(typeof(BatchJobResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportAccounts([FromQuery] ExportFormat format = ExportFormat.FixedWidth)
    {
        _logger.LogInformation("Account export job triggered in {Format} format", format);
        
        var result = await _dataExportImportService.ExportAccountsAsync(format);
        _jobHistory.Add(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Export transactions data (CBEXPORT equivalent for TRANSACT)
    /// </summary>
    [HttpPost("export/transactions")]
    [ProducesResponseType(typeof(BatchJobResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportTransactions(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] ExportFormat format = ExportFormat.FixedWidth)
    {
        _logger.LogInformation("Transaction export job triggered from {From} to {To}", fromDate, toDate);
        
        var result = await _dataExportImportService.ExportTransactionsAsync(fromDate, toDate, format);
        _jobHistory.Add(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Export customers data (CBEXPORT equivalent for CUSTDAT)
    /// </summary>
    [HttpPost("export/customers")]
    [ProducesResponseType(typeof(BatchJobResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportCustomers([FromQuery] ExportFormat format = ExportFormat.FixedWidth)
    {
        _logger.LogInformation("Customer export job triggered in {Format} format", format);
        
        var result = await _dataExportImportService.ExportCustomersAsync(format);
        _jobHistory.Add(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Run all nightly batch jobs (full batch cycle)
    /// </summary>
    [HttpPost("run-nightly-batch")]
    [ProducesResponseType(typeof(NightlyBatchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RunNightlyBatch()
    {
        _logger.LogInformation("Nightly batch cycle triggered");
        
        var nightlyResult = new NightlyBatchResult
        {
            StartTime = DateTime.UtcNow
        };

        // 1. Post transactions (CBTRN01C/CBTRN02C)
        nightlyResult.TransactionPosting = await _transactionPostingService.PostTransactionsAsync();
        _jobHistory.Add(nightlyResult.TransactionPosting);

        // 2. Calculate interest (CBACT02C)
        nightlyResult.InterestCalculation = await _interestCalculationService.CalculateInterestAsync();
        _jobHistory.Add(nightlyResult.InterestCalculation);

        // 3. Generate statements if end of month
        if (DateTime.UtcNow.Day == DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month))
        {
            nightlyResult.StatementGeneration = await _statementGenerationService.GenerateStatementsAsync(
                DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            _jobHistory.Add(nightlyResult.StatementGeneration);
        }

        nightlyResult.EndTime = DateTime.UtcNow;
        
        _logger.LogInformation("Nightly batch cycle completed in {Duration}", nightlyResult.Duration);

        return Ok(nightlyResult);
    }

    /// <summary>
    /// Get job execution history
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IEnumerable<BatchJobResult>), StatusCodes.Status200OK)]
    public IActionResult GetJobHistory([FromQuery] int limit = 50)
    {
        var history = _jobHistory
            .OrderByDescending(j => j.StartTime)
            .Take(limit)
            .ToList();
        
        return Ok(history);
    }

    /// <summary>
    /// Get a specific job result by ID
    /// </summary>
    [HttpGet("{jobId}")]
    [ProducesResponseType(typeof(BatchJobResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetJob(string jobId)
    {
        var job = _jobHistory.FirstOrDefault(j => j.JobId == jobId);
        
        if (job == null)
        {
            return NotFound(new { message = $"Job {jobId} not found" });
        }
        
        return Ok(job);
    }
}

public class NightlyBatchResult
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public BatchJobResult? TransactionPosting { get; set; }
    public BatchJobResult? InterestCalculation { get; set; }
    public BatchJobResult? StatementGeneration { get; set; }
    
    public bool AllSucceeded =>
        (TransactionPosting?.Status == BatchJobStatus.Completed || TransactionPosting?.Status == BatchJobStatus.CompletedWithErrors) &&
        (InterestCalculation?.Status == BatchJobStatus.Completed || InterestCalculation?.Status == BatchJobStatus.CompletedWithErrors) &&
        (StatementGeneration == null || StatementGeneration.Status == BatchJobStatus.Completed || StatementGeneration.Status == BatchJobStatus.CompletedWithErrors);
}
