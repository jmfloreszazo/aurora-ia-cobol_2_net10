namespace CardDemo.Application.Common.DTOs;

public class CustomerDto
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public string SSN { get; set; } = default!;
    public string GovernmentId { get; set; } = default!;
    public string PhoneNumber1 { get; set; } = default!;
    public string? PhoneNumber2 { get; set; }
    public string AddressLine1 { get; set; } = default!;
    public string? AddressLine2 { get; set; }
    public string StateCode { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
    public string CountryCode { get; set; } = default!;
    public int FICOScore { get; set; }
    public int NumberOfAccounts { get; set; }
}

public record CustomerResponse(
    int CustomerId,
    string FullName,
    string AddressLine1,
    string? AddressLine2,
    string StateCode,
    string CountryCode,
    string ZipCode,
    string PhoneNumber1,
    int FICOScore,
    int Age
);

public record CustomerDetailResponse(
    int CustomerId,
    string FirstName,
    string? MiddleName,
    string LastName,
    string FullName,
    string AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string StateCode,
    string CountryCode,
    string ZipCode,
    string PhoneNumber1,
    string? PhoneNumber2,
    string SSN,
    string GovernmentId,
    DateTime DateOfBirth,
    int Age,
    int FICOScore,
    string? EFTAccountId,
    bool PrimaryCardHolder,
    List<AccountResponse> Accounts
);

public record CreateCustomerRequest(
    string FirstName,
    string? MiddleName,
    string LastName,
    string AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string StateCode,
    string CountryCode,
    string ZipCode,
    string PhoneNumber1,
    string? PhoneNumber2,
    string SSN,
    string GovernmentId,
    DateTime DateOfBirth,
    int FICOScore,
    string? EFTAccountId
);

public record UpdateCustomerRequest(
    string? AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string? ZipCode,
    string? PhoneNumber1,
    string? PhoneNumber2,
    int? FICOScore
);
