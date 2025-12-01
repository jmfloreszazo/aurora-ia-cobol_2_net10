using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Customers.Queries;

public record GetCustomerByIdQuery(int CustomerId) : IRequest<CustomerDto?>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly ICardDemoDbContext _context;

    public GetCustomerByIdQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .Include(c => c.Accounts)
            .Where(c => c.CustomerId == request.CustomerId)
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
            .FirstOrDefaultAsync(cancellationToken);

        return customer;
    }
}
