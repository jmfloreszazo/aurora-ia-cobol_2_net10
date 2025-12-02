using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Cards.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Features.Cards.Commands;

public record UpdateCardCommand(
    string CardNumber,
    string? CardType,
    string? EmbossedName,
    string? ExpirationDate,
    string? ActiveStatus) : IRequest<CardDto>;

public class UpdateCardCommandHandler : IRequestHandler<UpdateCardCommand, CardDto>
{
    private readonly ICardDemoDbContext _context;

    public UpdateCardCommandHandler(ICardDemoDbContext context)
    {
        _context = context;
    }

    public async Task<CardDto> Handle(UpdateCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _context.Cards
            .FirstOrDefaultAsync(c => c.CardNumber == request.CardNumber, cancellationToken);

        if (card == null)
        {
            throw new KeyNotFoundException($"Card {request.CardNumber} not found");
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.CardType))
        {
            card.CardType = request.CardType;
        }

        if (!string.IsNullOrEmpty(request.EmbossedName))
        {
            card.EmbossedName = request.EmbossedName;
        }

        if (!string.IsNullOrEmpty(request.ExpirationDate))
        {
            card.ExpirationDate = request.ExpirationDate;
        }

        if (!string.IsNullOrEmpty(request.ActiveStatus))
        {
            card.ActiveStatus = request.ActiveStatus;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new CardDto
        {
            CardNumber = card.CardNumber,
            MaskedCardNumber = card.CardNumber.Length >= 4 
                ? $"**** **** **** {card.CardNumber.Substring(card.CardNumber.Length - 4)}" 
                : card.CardNumber,
            AccountId = card.AccountId,
            CardType = card.CardType,
            EmbossedName = card.EmbossedName,
            ExpirationDate = card.ExpirationDate,
            ActiveStatus = card.ActiveStatus
        };
    }
}
