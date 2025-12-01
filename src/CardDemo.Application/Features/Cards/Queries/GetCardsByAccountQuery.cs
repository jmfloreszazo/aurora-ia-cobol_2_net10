using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Cards.Queries;

public record GetCardsByAccountQuery(long AccountId) : IRequest<List<CardDto>>;

public class GetCardsByAccountQueryHandler : IRequestHandler<GetCardsByAccountQuery, List<CardDto>>
{
    private readonly ICardDemoDbContext _context;

    public GetCardsByAccountQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<List<CardDto>> Handle(GetCardsByAccountQuery request, CancellationToken cancellationToken)
    {
        var cards = await _context.Cards
            .Include(c => c.Account)
            .Where(c => c.AccountId == request.AccountId)
            .Select(c => new CardDto
            {
                CardNumber = c.CardNumber,
                MaskedCardNumber = c.CardNumber.Length >= 4 ? $"**** **** **** {c.CardNumber.Substring(c.CardNumber.Length - 4)}" : c.CardNumber,
                AccountId = c.AccountId,
                CardType = c.CardType,
                EmbossedName = c.EmbossedName,
                ExpirationDate = c.ExpirationDate,
                ActiveStatus = c.ActiveStatus,
                IsActive = c.ActiveStatus == "Y"
            })
            .ToListAsync(cancellationToken);

        return cards;
    }
}
