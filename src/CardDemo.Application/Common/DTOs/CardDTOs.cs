namespace CardDemo.Application.Common.DTOs;

public class CardDto
{
    public string CardNumber { get; set; } = default!;
    public string MaskedCardNumber { get; set; } = default!;
    public long AccountId { get; set; }
    public string CardType { get; set; } = default!;
    public string EmbossedName { get; set; } = default!;
    public string ExpirationDate { get; set; } = default!;
    public string ActiveStatus { get; set; } = default!;
    public bool IsActive { get; set; }
}

public record CardResponse(
    string CardNumber,
    string MaskedCardNumber,
    long AccountId,
    string CardType,
    string EmbossedName,
    string ExpirationDate,
    string ActiveStatus,
    bool IsExpired
);

public record CardSummaryResponse(
    string CardNumber,
    string MaskedCardNumber,
    string CardType,
    string ExpirationDate,
    string ActiveStatus
);

public record CreateCardRequest(
    string CardNumber,
    long AccountId,
    string CardType,
    string EmbossedName,
    string ExpirationDate
);

public record UpdateCardRequest(
    string? ActiveStatus,
    string? ExpirationDate
);
