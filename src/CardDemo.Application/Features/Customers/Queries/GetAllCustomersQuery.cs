using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Customers.Queries;

public record GetAllCustomersQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<CustomerDto>>;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, PagedResult<CustomerDto>>
{
    private readonly ICardDemoDbContext _context;

    public GetAllCustomersQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers
            .Include(c => c.Accounts)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName);

        var totalCount = await query.CountAsync(cancellationToken);

        var customers = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                FirstName = c.FirstName,
                MiddleName = c.MiddleName,
                LastName = c.LastName,
                FullName = c.MiddleName != null ? $"{c.FirstName} {c.MiddleName} {c.LastName}" : $"{c.FirstName} {c.LastName}",
                DateOfBirth = c.DateOfBirth,
                SSN = c.SSN,
                GovernmentId = c.GovernmentId,
                PhoneNumber1 = c.PhoneNumber1,
                PhoneNumber2 = c.PhoneNumber2,
                AddressLine1 = c.AddressLine1,
                AddressLine2 = c.AddressLine2,
                StateCode = c.StateCode,
                ZipCode = c.ZipCode,
                CountryCode = c.CountryCode,
                FICOScore = c.FICOScore,
                NumberOfAccounts = c.Accounts.Count
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<CustomerDto>
        {
            Items = customers,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
