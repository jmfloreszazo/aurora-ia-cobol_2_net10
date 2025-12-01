using CardDemo.Domain.Common;

namespace CardDemo.Domain.Entities;

public class Transaction : BaseEntity
{
    public string TransactionId { get; set; } = default!;
    public long AccountId { get; set; }
    public string CardNumber { get; set; } = default!;
    public string TransactionType { get; set; } = default!;
    public int CategoryCode { get; set; }
    public string TransactionSource { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Amount { get; set; }
    public string? MerchantId { get; set; }
    public string? MerchantName { get; set; }
    public string? MerchantCity { get; set; }
    public string? MerchantZip { get; set; }
    public string? OrigTransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string ProcessedFlag { get; set; } = "N";

    // Navigation properties
    public Account Account { get; set; } = default!;
    public Card Card { get; set; } = default!;
    public TransactionType TransactionTypeNavigation { get; set; } = default!;
    public TransactionCategory Category { get; set; } = default!;

    // Computed properties
    public bool IsProcessed => ProcessedFlag == "Y";
    public bool IsDebit => Amount < 0;
    public bool IsCredit => Amount > 0;
    public bool IsReversal => !string.IsNullOrEmpty(OrigTransactionId);
}
