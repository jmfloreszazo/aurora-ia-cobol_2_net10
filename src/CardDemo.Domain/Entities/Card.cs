using CardDemo.Domain.Common;

namespace CardDemo.Domain.Entities;

public class Card : BaseEntity
{
    public string CardNumber { get; set; } = default!;
    public long AccountId { get; set; }
    public string CardType { get; set; } = default!;
    public string EmbossedName { get; set; } = default!;
    public string ExpirationDate { get; set; } = default!; // Format: MM/YYYY
    public string ActiveStatus { get; set; } = "Y";

    // Navigation properties
    public Account Account { get; set; } = default!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    // Computed properties
    public bool IsActive => ActiveStatus == "Y";
    public bool IsExpired
    {
        get
        {
            if (string.IsNullOrEmpty(ExpirationDate) || !ExpirationDate.Contains('/'))
                return true;

            var parts = ExpirationDate.Split('/');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
                return true;

            var expDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            return DateTime.UtcNow > expDate;
        }
    }

    public string MaskedCardNumber => CardNumber.Length >= 4 
        ? $"**** **** **** {CardNumber[^4..]}" 
        : CardNumber;
}
