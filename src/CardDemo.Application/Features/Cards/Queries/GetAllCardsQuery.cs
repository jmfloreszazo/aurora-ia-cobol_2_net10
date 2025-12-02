using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Cards.Queries;

public record GetAllCardsQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<CardDto>>;

public record CardDto
{
    public string CardNumber { get; init; } = default!;
    public string MaskedCardNumber { get; init; } = default!;
    public long AccountId { get; init; }
    public string CardType { get; init; } = default!;
    public string EmbossedName { get; init; } = default!;
    public string ExpirationDate { get; init; } = default!;
    public string ActiveStatus { get; init; } = default!;
    public bool IsActive => ActiveStatus == "Y";
}

public class GetAllCardsQueryHandler : IRequestHandler<GetAllCardsQuery, PagedResult<CardDto>>
{
    private readonly ICardDemoDbContext _context;

    public GetAllCardsQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CardDto>> Handle(GetAllCardsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Cards.OrderBy(c => c.CardNumber);

        var totalCount = await query.CountAsync(cancellationToken);

        var cards = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CardDto
            {
                CardNumber = c.CardNumber,
                MaskedCardNumber = c.CardNumber.Length >= 4 
                    ? $"**** **** **** {c.CardNumber.Substring(c.CardNumber.Length - 4)}" 
                    : c.CardNumber,
                AccountId = c.AccountId,
                CardType = c.CardType,
                EmbossedName = c.EmbossedName,
                ExpirationDate = c.ExpirationDate,
                ActiveStatus = c.ActiveStatus
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<CardDto>
        {
            Items = cards,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
