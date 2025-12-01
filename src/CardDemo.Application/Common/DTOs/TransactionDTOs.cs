namespace CardDemo.Application.Common.DTOs;

public class TransactionDto
{
    public string TransactionId { get; set; } = default!;
    public long AccountId { get; set; }
    public string CardNumber { get; set; } = default!;
    public string MaskedCardNumber { get; set; } = default!;
    public string TransactionType { get; set; } = default!;
    public string TransactionTypeDescription { get; set; } = default!;
    public int CategoryCode { get; set; }
    public string CategoryDescription { get; set; } = default!;
    public string TransactionSource { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Amount { get; set; }
    public string? MerchantId { get; set; }
    public string? MerchantName { get; set; }
    public string? MerchantCity { get; set; }
    public DateTime TransactionDate { get; set; }
    public string ProcessedFlag { get; set; } = default!;
    public bool IsProcessed { get; set; }
}

public record TransactionResponse(
    string TransactionId,
    long AccountId,
    string CardNumber,
    string MaskedCardNumber,
    string TransactionType,
    string TransactionTypeDescription,
    int CategoryCode,
    string CategoryDescription,
    string TransactionSource,
    string Description,
    decimal Amount,
    string? MerchantId,
    string? MerchantName,
    string? MerchantCity,
    string? MerchantZip,
    string? OrigTransactionId,
    DateTime TransactionDate,
    bool IsProcessed
);

public record TransactionSummaryResponse(
    string TransactionId,
    string CardNumber,
    string MaskedCardNumber,
    string Description,
    decimal Amount,
    DateTime TransactionDate
);

public record CreateTransactionRequest(
    long AccountId,
    string CardNumber,
    string TransactionType,
    int CategoryCode,
    string TransactionSource,
    string Description,
    decimal Amount,
    string? MerchantId,
    string? MerchantName,
    string? MerchantCity,
    string? MerchantZip,
    DateTime? TransactionDate
);

public record TransactionFilterRequest(
    long? AccountId,
    string? CardNumber,
    DateTime? StartDate,
    DateTime? EndDate,
    string? TransactionType,
    int? CategoryCode,
    decimal? MinAmount,
    decimal? MaxAmount,
    int PageNumber = 1,
    int PageSize = 50
);
