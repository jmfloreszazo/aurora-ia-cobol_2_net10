namespace CardDemo.Application.Features.Payments;

public record PaymentDto
{
    public string ConfirmationNumber { get; init; } = default!;
    public DateTime Timestamp { get; init; }
    public long AccountId { get; init; }
    public decimal Amount { get; init; }
    public decimal NewBalance { get; init; }
    public string TransactionId { get; init; } = default!;
}

public record MakePaymentRequest
{
    public long AccountId { get; init; }
    public decimal Amount { get; init; }
    public DateTime? PaymentDate { get; init; }
}
