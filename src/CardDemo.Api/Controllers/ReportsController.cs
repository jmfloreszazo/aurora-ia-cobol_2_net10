using CardDemo.Application.Features.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardDemo.Api.Controllers;

/// <summary>
/// Controller for generating transaction reports (equivalent to CORPT00C COBOL program)
/// Supports Monthly, Yearly, and Custom date range reports
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IMediator mediator, ILogger<ReportsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get monthly transaction report
    /// </summary>
    /// <param name="year">Year for the report</param>
    /// <param name="month">Month for the report (1-12)</param>
    /// <param name="accountId">Optional: Filter by account ID</param>
    [HttpGet("monthly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMonthlyReport(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] long? accountId = null)
    {
        if (month < 1 || month > 12)
        {
            return BadRequest(new { message = "Month must be between 1 and 12" });
        }

        if (year < 1900 || year > 2100)
        {
            return BadRequest(new { message = "Year must be between 1900 and 2100" });
        }

        _logger.LogInformation("Generating monthly report for {Year}-{Month:D2}", year, month);
        
        var query = new GetMonthlyReportQuery(year, month, accountId);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    /// <summary>
    /// Get yearly transaction report
    /// </summary>
    /// <param name="year">Year for the report</param>
    /// <param name="accountId">Optional: Filter by account ID</param>
    [HttpGet("yearly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetYearlyReport(
        [FromQuery] int year,
        [FromQuery] long? accountId = null)
    {
        if (year < 1900 || year > 2100)
        {
            return BadRequest(new { message = "Year must be between 1900 and 2100" });
        }

        _logger.LogInformation("Generating yearly report for {Year}", year);
        
        var query = new GetYearlyReportQuery(year, accountId);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    /// <summary>
    /// Get custom date range transaction report
    /// </summary>
    /// <param name="startDate">Start date (YYYY-MM-DD)</param>
    /// <param name="endDate">End date (YYYY-MM-DD)</param>
    /// <param name="accountId">Optional: Filter by account ID</param>
    [HttpGet("custom")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCustomReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] long? accountId = null)
    {
        if (startDate > endDate)
        {
            return BadRequest(new { message = "Start date must be before or equal to end date" });
        }

        _logger.LogInformation("Generating custom report from {StartDate} to {EndDate}", 
            startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
        
        var query = new GetCustomReportQuery(startDate, endDate, accountId);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
}
