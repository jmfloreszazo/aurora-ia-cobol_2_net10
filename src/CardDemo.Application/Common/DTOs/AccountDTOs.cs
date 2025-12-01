namespace CardDemo.Application.Common.DTOs;

public record AccountDto(
    long AccountId,
    int CustomerId,
    string CustomerName,
    string ActiveStatus,
    decimal CurrentBalance,
    decimal CreditLimit,
    decimal CashCreditLimit,
    decimal AvailableCredit,
    decimal CreditUtilization,
    DateTime OpenDate,
    DateTime ExpirationDate,
    decimal CurrentCycleCredit,
    decimal CurrentCycleDebit,
    int NumberOfCards
)
{
    public AccountDto() : this(0, 0, string.Empty, string.Empty, 0, 0, 0, 0, 0, DateTime.MinValue, DateTime.MinValue, 0, 0, 0) { }
    
    public long AccountId { get; init; } = AccountId;
    public int CustomerId { get; init; } = CustomerId;
    public string CustomerName { get; set; } = CustomerName;
    public string ActiveStatus { get; set; } = ActiveStatus;
    public decimal CurrentBalance { get; set; } = CurrentBalance;
    public decimal CreditLimit { get; set; } = CreditLimit;
    public decimal CashCreditLimit { get; set; } = CashCreditLimit;
    public decimal AvailableCredit { get; set; } = AvailableCredit;
    public decimal CreditUtilization { get; set; } = CreditUtilization;
    public DateTime OpenDate { get; set; } = OpenDate;
    public DateTime ExpirationDate { get; set; } = ExpirationDate;
    public decimal CurrentCycleCredit { get; set; } = CurrentCycleCredit;
    public decimal CurrentCycleDebit { get; set; } = CurrentCycleDebit;
    public int NumberOfCards { get; set; } = NumberOfCards;
}

public record AccountResponse(
    long AccountId,
    int CustomerId,
    string CustomerName,
    string ActiveStatus,
    decimal CurrentBalance,
    decimal CreditLimit,
    decimal AvailableCredit,
    decimal CreditUtilization,
    DateTime OpenDate,
    DateTime ExpirationDate,
    string ZipCode
);

public record AccountDetailResponse(
    long AccountId,
    int CustomerId,
    string CustomerName,
    string ActiveStatus,
    decimal CurrentBalance,
    decimal CreditLimit,
    decimal CashCreditLimit,
    decimal AvailableCredit,
    decimal CreditUtilization,
    DateTime OpenDate,
    DateTime ExpirationDate,
    DateTime? ReissueDate,
    decimal CurrentCycleCredit,
    decimal CurrentCycleDebit,
    string ZipCode,
    string? GroupId,
    List<CardSummaryResponse> Cards,
    List<TransactionSummaryResponse> RecentTransactions
);

public record CreateAccountRequest(
    int CustomerId,
    decimal CreditLimit,
    decimal CashCreditLimit,
    DateTime OpenDate,
    DateTime ExpirationDate,
    string ZipCode,
    string? GroupId
);

public record UpdateAccountRequest(
    decimal? CreditLimit,
    decimal? CashCreditLimit,
    DateTime? ExpirationDate,
    string? ActiveStatus
);
