using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Reports.Queries;

public record ReportSummaryDto
{
    public string? Month { get; init; }
    public int Year { get; init; }
    public int TransactionCount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal AvgAmount { get; init; }
}

public record GetMonthlyReportQuery(int Year, int Month, long? AccountId = null) : IRequest<List<ReportSummaryDto>>;

public class GetMonthlyReportQueryHandler : IRequestHandler<GetMonthlyReportQuery, List<ReportSummaryDto>>
{
    private readonly ICardDemoDbContext _context;

    public GetMonthlyReportQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReportSummaryDto>> Handle(GetMonthlyReportQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateTime(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var query = _context.Transactions
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);

        if (request.AccountId.HasValue)
        {
            query = query.Where(t => t.AccountId == request.AccountId.Value);
        }

        var summary = await query
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new ReportSummaryDto
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Year = g.Key.Year,
                TransactionCount = g.Count(),
                TotalAmount = g.Sum(t => t.Amount),
                AvgAmount = g.Average(t => t.Amount)
            })
            .ToListAsync(cancellationToken);

        return summary;
    }
}
