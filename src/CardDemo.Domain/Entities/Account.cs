using CardDemo.Domain.Common;

namespace CardDemo.Domain.Entities;

public class Account : BaseEntity
{
    public long AccountId { get; set; }
    public int CustomerId { get; set; }
    public string ActiveStatus { get; set; } = "Y";
    public decimal CurrentBalance { get; set; } = 0.00m;
    public decimal CreditLimit { get; set; }
    public decimal CashCreditLimit { get; set; }
    public DateTime OpenDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DateTime? ReissueDate { get; set; }
    public decimal CurrentCycleCredit { get; set; } = 0.00m;
    public decimal CurrentCycleDebit { get; set; } = 0.00m;
    public string ZipCode { get; set; } = default!;
    public string? GroupId { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = default!;
    public ICollection<Card> Cards { get; set; } = new List<Card>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    // Computed properties
    public bool IsActive => ActiveStatus == "Y";
    public decimal AvailableCredit => CreditLimit - CurrentBalance;
    public decimal CreditUtilization => CreditLimit > 0 ? (CurrentBalance / CreditLimit) * 100 : 0;
}
