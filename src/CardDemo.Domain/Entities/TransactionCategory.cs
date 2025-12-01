using CardDemo.Domain.Common;

namespace CardDemo.Domain.Entities;

public class TransactionCategory : BaseEntity
{
    public int CategoryCode { get; set; }
    public string CategoryDescription { get; set; } = default!;

    // Navigation properties
    public ICollection<TransactionType> TransactionTypes { get; set; } = new List<TransactionType>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
