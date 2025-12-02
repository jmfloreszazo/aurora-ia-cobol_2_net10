using CardDemo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Cards.Queries;

public record GetCardByNumberQuery(string CardNumber) : IRequest<CardDto?>;

public class GetCardByNumberQueryHandler : IRequestHandler<GetCardByNumberQuery, CardDto?>
{
    private readonly ICardDemoDbContext _context;

    public GetCardByNumberQueryHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<CardDto?> Handle(GetCardByNumberQuery request, CancellationToken cancellationToken)
    {
        var card = await _context.Cards
            .Where(c => c.CardNumber == request.CardNumber)
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
            .FirstOrDefaultAsync(cancellationToken);

        return card;
    }
}
