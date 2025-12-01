using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Reports.Queries;

public record GetCustomReportQuery(
    DateTime StartDate,
    DateTime EndDate,
    long? AccountId = null) : IRequest<List<ReportSummaryDto>>;

public class GetCustomReportQueryHandler : IRequestHandler<GetCustomReportQuery, List<ReportSummaryDto>>
{
    private readonly ICardDemoDbContext _context;

    public GetCustomReportQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReportSummaryDto>> Handle(GetCustomReportQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transactions
            .Where(t => t.TransactionDate >= request.StartDate && t.TransactionDate <= request.EndDate);

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
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ToListAsync(cancellationToken);

        return summary;
    }
}
