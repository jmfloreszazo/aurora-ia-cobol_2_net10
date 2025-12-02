using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.BatchJobs;
using CardDemo.Application.Features.BatchJobs.Services;
using CardDemo.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.BatchJobs;

public class InterestCalculationServiceTests
{
    private readonly Mock<ILogger<InterestCalculationService>> _loggerMock;
    private readonly CardDemoTestDbContext _dbContext;
    private readonly InterestCalculationService _service;

    public InterestCalculationServiceTests()
    {
        _loggerMock = new Mock<ILogger<InterestCalculationService>>();
        
        var options = new DbContextOptionsBuilder<CardDemoTestDbContext>()
            .UseInMemoryDatabase(databaseName: $"InterestCalcTest_{Guid.NewGuid()}")
            .Options;
        
        _dbContext = new CardDemoTestDbContext(options);
        _service = new InterestCalculationService(_dbContext, _loggerMock.Object);
    }

    private Customer CreateTestCustomer(int id = 1)
    {
        return new Customer
        {
            CustomerId = id,
            FirstName = "Test",
            LastName = "Customer",
            AddressLine1 = "123 Test St",
            StateCode = "TX",
            CountryCode = "USA",
            ZipCode = "12345",
            PhoneNumber1 = "555-1234",
            SSN = "123-45-6789",
            GovernmentId = "DL123456",
            DateOfBirth = new DateTime(1980, 1, 1),
            FICOScore = 750
        };
    }

    private Account CreateTestAccount(int customerId, long accountId, decimal balance = 1000m, string activeStatus = "Y")
    {
        return new Account
        {
            AccountId = accountId,
            CustomerId = customerId,
            ActiveStatus = activeStatus,
            CurrentBalance = balance,
            CreditLimit = 5000m,
            CashCreditLimit = 1000m,
            CurrentCycleDebit = 0m,
            CurrentCycleCredit = 0m,
            OpenDate = DateTime.UtcNow.AddYears(-1),
            ExpirationDate = DateTime.UtcNow.AddYears(2),
            ZipCode = "12345"
        };
    }

    [Fact]
    public async Task CalculateInterestAsync_ShouldReturnCompletedResult_WhenNoAccountsWithBalance()
    {
        // Arrange - no accounts in database
        
        // Act
        var result = await _service.CalculateInterestAsync();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(BatchJobStatus.Completed);
        result.JobName.Should().Be("INTEREST-CALCULATION");
        result.RecordsProcessed.Should().Be(0);
    }

    [Fact]
    public async Task CalculateInterestAsync_ShouldCalculateForActiveAccountsWithBalance()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId, 1, balance: 1000m);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CalculateInterestAsync();

        // Assert
        result.Status.Should().Be(BatchJobStatus.Completed);
        result.RecordsProcessed.Should().Be(1);
        result.RecordsSucceeded.Should().Be(1);
        result.RecordsFailed.Should().Be(0);
    }

    [Fact]
    public async Task CalculateInterestAsync_ShouldSkipAccountsWithZeroBalance()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var accountWithBalance = CreateTestAccount(customer.CustomerId, 1, balance: 500m);
        var accountWithZeroBalance = CreateTestAccount(customer.CustomerId, 2, balance: 0m);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddRangeAsync(accountWithBalance, accountWithZeroBalance);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CalculateInterestAsync();

        // Assert
        result.RecordsProcessed.Should().Be(1); // Only account with balance
    }

    [Fact]
    public async Task CalculateInterestAsync_ShouldSkipInactiveAccounts()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var activeAccount = CreateTestAccount(customer.CustomerId, 1, balance: 500m, activeStatus: "Y");
        var inactiveAccount = CreateTestAccount(customer.CustomerId, 2, balance: 500m, activeStatus: "N");

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddRangeAsync(activeAccount, inactiveAccount);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CalculateInterestAsync();

        // Assert
        result.RecordsProcessed.Should().Be(1); // Only active account
    }

    [Fact]
    public async Task CalculateInterestAsync_ShouldCreateInterestTransaction()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId, 1, balance: 1000m);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();

        var initialTransactionCount = await _dbContext.Transactions.CountAsync();

        // Act
        await _service.CalculateInterestAsync();

        // Assert
        var transactionCount = await _dbContext.Transactions.CountAsync();
        transactionCount.Should().Be(initialTransactionCount + 1);

        var interestTransaction = await _dbContext.Transactions.FirstAsync();
        interestTransaction.TransactionType.Should().Be("IN");
        interestTransaction.TransactionSource.Should().Be("BATCH");
        interestTransaction.CardNumber.Should().Be("SYSTEM-INTEREST");
        interestTransaction.ProcessedFlag.Should().Be("Y");
        interestTransaction.Amount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CalculateInterestAsync_ShouldUpdateAccountBalance()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var initialBalance = 1000m;
        var account = CreateTestAccount(customer.CustomerId, 1, balance: initialBalance);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.CalculateInterestAsync();

        // Assert
        var updatedAccount = await _dbContext.Accounts.FirstAsync();
        updatedAccount.CurrentBalance.Should().BeGreaterThan(initialBalance);
    }

    [Fact]
    public async Task CalculateInterestAsync_ShouldUseCorrectDailyRate()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var balance = 10000m;
        var account = CreateTestAccount(customer.CustomerId, 1, balance: balance);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.CalculateInterestAsync();

        // Assert
        // Expected daily interest: balance * (APR / 365) = 10000 * (0.1999 / 365) ≈ 5.48
        var interestTransaction = await _dbContext.Transactions.FirstAsync();
        var expectedDailyInterest = Math.Round(balance * (0.1999m / 365m), 2);
        interestTransaction.Amount.Should().BeApproximately(expectedDailyInterest, 0.01m);
    }

    [Fact]
    public async Task CalculateInterestAsync_ShouldAcceptOptionalDate()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId, 1, balance: 1000m);
        var specificDate = new DateTime(2024, 6, 15);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CalculateInterestAsync(specificDate);

        // Assert
        result.Status.Should().Be(BatchJobStatus.Completed);
        
        var interestTransaction = await _dbContext.Transactions.FirstAsync();
        interestTransaction.TransactionDate.Should().Be(specificDate);
        interestTransaction.Description.Should().Contain("2024-06-15");
    }

    [Fact]
    public async Task CalculateInterestAsync_ShouldReturnCorrectRecordCounts()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account1 = CreateTestAccount(customer.CustomerId, 1, balance: 500m);
        var account2 = CreateTestAccount(customer.CustomerId, 2, balance: 1500m);
        var account3 = CreateTestAccount(customer.CustomerId, 3, balance: 2000m);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddRangeAsync(account1, account2, account3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CalculateInterestAsync();

        // Assert
        result.RecordsProcessed.Should().Be(3);
        result.RecordsSucceeded.Should().Be(3);
        result.RecordsFailed.Should().Be(0);
    }

    [Fact]
    public async Task CalculateInterestAsync_ShouldUpdateCycleDebit()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId, 1, balance: 1000m);
        account.CurrentCycleDebit = 100m;

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.CalculateInterestAsync();

        // Assert
        var updatedAccount = await _dbContext.Accounts.FirstAsync();
        updatedAccount.CurrentCycleDebit.Should().BeGreaterThan(100m);
    }

    [Fact]
    public async Task CalculateInterestAsync_ShouldProcessMultipleAccountsIndependently()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account1 = CreateTestAccount(customer.CustomerId, 1, balance: 1000m);
        var account2 = CreateTestAccount(customer.CustomerId, 2, balance: 2000m);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddRangeAsync(account1, account2);
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.CalculateInterestAsync();

        // Assert
        var transactions = await _dbContext.Transactions.ToListAsync();
        transactions.Should().HaveCount(2);
        
        var firstInterest = transactions.First(t => t.AccountId == 1);
        var secondInterest = transactions.First(t => t.AccountId == 2);
        
        // Second should have higher interest due to higher balance
        secondInterest.Amount.Should().BeGreaterThan(firstInterest.Amount);
    }

    [Fact]
    public async Task CalculateInterestAsync_ShouldGenerateUniqueTransactionIds()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account1 = CreateTestAccount(customer.CustomerId, 1, balance: 1000m);
        var account2 = CreateTestAccount(customer.CustomerId, 2, balance: 1000m);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddRangeAsync(account1, account2);
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.CalculateInterestAsync();

        // Assert
        var transactions = await _dbContext.Transactions.ToListAsync();
        var transactionIds = transactions.Select(t => t.TransactionId).ToList();
        transactionIds.Should().OnlyHaveUniqueItems();
    }
}
