using CardDemo.Domain.Common;

namespace CardDemo.Domain.Entities;

public class TransactionType : BaseEntity
{
    public string TypeCode { get; set; } = default!;
    public string TypeDescription { get; set; } = default!;
    public int CategoryCode { get; set; }

    // Navigation properties
    public TransactionCategory Category { get; set; } = default!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
