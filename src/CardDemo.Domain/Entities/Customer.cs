using CardDemo.Domain.Common;

namespace CardDemo.Domain.Entities;

public class Customer : BaseEntity
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public string AddressLine1 { get; set; } = default!;
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string StateCode { get; set; } = default!;
    public string CountryCode { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
    public string PhoneNumber1 { get; set; } = default!;
    public string? PhoneNumber2 { get; set; }
    public string SSN { get; set; } = default!;
    public string GovernmentId { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public int FICOScore { get; set; }
    public string? EFTAccountId { get; set; }
    public bool PrimaryCardHolder { get; set; } = true;

    // Navigation properties
    public ICollection<Account> Accounts { get; set; } = new List<Account>();

    public string FullName => string.IsNullOrEmpty(MiddleName) 
        ? $"{FirstName} {LastName}" 
        : $"{FirstName} {MiddleName} {LastName}";

    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year;
}
