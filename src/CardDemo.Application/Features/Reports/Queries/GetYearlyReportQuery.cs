using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Reports.Queries;

public record GetYearlyReportQuery(int Year, long? AccountId = null) : IRequest<List<ReportSummaryDto>>;

public class GetYearlyReportQueryHandler : IRequestHandler<GetYearlyReportQuery, List<ReportSummaryDto>>
{
    private readonly ICardDemoDbContext _context;

    public GetYearlyReportQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReportSummaryDto>> Handle(GetYearlyReportQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateTime(request.Year, 1, 1);
        var endDate = new DateTime(request.Year, 12, 31);

        var query = _context.Transactions
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);

        if (request.AccountId.HasValue)
        {
            query = query.Where(t => t.AccountId == request.AccountId.Value);
        }

        var summary = await query
            .GroupBy(t => t.TransactionDate.Month)
            .Select(g => new ReportSummaryDto
            {
                Month = $"{request.Year}-{g.Key:D2}",
                Year = request.Year,
                TransactionCount = g.Count(),
                TotalAmount = g.Sum(t => t.Amount),
                AvgAmount = g.Average(t => t.Amount)
            })
            .OrderBy(r => r.Month)
            .ToListAsync(cancellationToken);

        return summary;
    }
}
