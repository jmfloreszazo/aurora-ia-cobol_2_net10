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

public class TransactionPostingServiceTests
{
    private readonly Mock<ILogger<TransactionPostingService>> _loggerMock;
    private readonly CardDemoTestDbContext _dbContext;
    private readonly TransactionPostingService _service;

    public TransactionPostingServiceTests()
    {
        _loggerMock = new Mock<ILogger<TransactionPostingService>>();
        
        var options = new DbContextOptionsBuilder<CardDemoTestDbContext>()
            .UseInMemoryDatabase(databaseName: $"TransactionPostingTest_{Guid.NewGuid()}")
            .Options;
        
        _dbContext = new CardDemoTestDbContext(options);
        _service = new TransactionPostingService(_dbContext, _loggerMock.Object);
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

    private Account CreateTestAccount(int customerId, long accountId = 1, decimal balance = 1000m, decimal limit = 5000m)
    {
        return new Account
        {
            AccountId = accountId,
            CustomerId = customerId,
            ActiveStatus = "Y",
            CurrentBalance = balance,
            CreditLimit = limit,
            CashCreditLimit = 1000m,
            OpenDate = DateTime.UtcNow.AddYears(-1),
            ExpirationDate = DateTime.UtcNow.AddYears(2),
            ZipCode = "12345"
        };
    }

    private Card CreateTestCard(long accountId, string cardNumber = "4000123456789012", bool active = true, bool expired = false)
    {
        var expDate = expired 
            ? $"{DateTime.UtcNow.AddMonths(-1):MM}/{DateTime.UtcNow.AddMonths(-1):yyyy}"
            : $"{DateTime.UtcNow.AddYears(2):MM}/{DateTime.UtcNow.AddYears(2):yyyy}";
        
        return new Card
        {
            CardNumber = cardNumber,
            AccountId = accountId,
            CardType = "GOLD",
            ActiveStatus = active ? "Y" : "N",
            ExpirationDate = expDate,
            EmbossedName = "TEST CUSTOMER"
        };
    }

    private Transaction CreateTestTransaction(long accountId, string cardNumber, decimal amount, string processedFlag = "N")
    {
        return new Transaction
        {
            TransactionId = $"TXN{Guid.NewGuid():N}".Substring(0, 16),
            AccountId = accountId,
            CardNumber = cardNumber,
            TransactionType = amount > 0 ? "PU" : "PA",
            CategoryCode = 1,
            TransactionSource = "ONLINE",
            Description = "Test Transaction",
            Amount = amount,
            MerchantName = "Test Merchant",
            TransactionDate = DateTime.UtcNow,
            ProcessedFlag = processedFlag
        };
    }

    [Fact]
    public async Task PostTransactionsAsync_ShouldReturnCompletedResult_WhenNoTransactionsToProcess()
    {
        // Act
        var result = await _service.PostTransactionsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(BatchJobStatus.Completed);
        result.RecordsProcessed.Should().Be(0);
        result.RecordsSucceeded.Should().Be(0);
        result.RecordsFailed.Should().Be(0);
    }

    [Fact]
    public async Task PostTransactionsAsync_ShouldProcessAllUnprocessedTransactions()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId, balance: 500m);
        var card = CreateTestCard(account.AccountId);
        var transaction = CreateTestTransaction(account.AccountId, card.CardNumber, 100m);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Cards.AddAsync(card);
        await _dbContext.Transactions.AddAsync(transaction);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.PostTransactionsAsync();

        // Assert
        result.Status.Should().Be(BatchJobStatus.Completed);
        result.RecordsProcessed.Should().Be(1);
        result.RecordsSucceeded.Should().Be(1);
        result.RecordsFailed.Should().Be(0);

        var processedTransaction = await _dbContext.Transactions.FirstAsync();
        processedTransaction.ProcessedFlag.Should().Be("Y");
    }

    [Fact]
    public async Task PostTransactionsAsync_ShouldUpdateAccountBalance()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId, balance: 500m);
        var card = CreateTestCard(account.AccountId);
        var transaction = CreateTestTransaction(account.AccountId, card.CardNumber, 200m);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Cards.AddAsync(card);
        await _dbContext.Transactions.AddAsync(transaction);
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.PostTransactionsAsync();

        // Assert
        var updatedAccount = await _dbContext.Accounts.FirstAsync();
        updatedAccount.CurrentBalance.Should().Be(700m); // 500 + 200
        updatedAccount.CurrentCycleDebit.Should().Be(200m);
    }

    [Fact]
    public async Task PostTransactionsAsync_ShouldFailWhenCardNotFound()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId);
        var transaction = CreateTestTransaction(account.AccountId, "NONEXISTENT", 100m);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Transactions.AddAsync(transaction);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.PostTransactionsAsync();

        // Assert
        result.RecordsFailed.Should().Be(1);
        result.Errors.Should().ContainMatch("*Card NONEXISTENT not found*");
    }

    [Fact]
    public async Task PostTransactionsAsync_ShouldFailWhenCardInactive()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId);
        var card = CreateTestCard(account.AccountId, active: false);
        var transaction = CreateTestTransaction(account.AccountId, card.CardNumber, 100m);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Cards.AddAsync(card);
        await _dbContext.Transactions.AddAsync(transaction);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.PostTransactionsAsync();

        // Assert
        result.RecordsFailed.Should().Be(1);
        result.Errors.Should().ContainMatch("*is not active*");
    }

    [Fact]
    public async Task PostTransactionsAsync_ShouldFailWhenCardExpired()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId);
        var card = CreateTestCard(account.AccountId, expired: true);
        var transaction = CreateTestTransaction(account.AccountId, card.CardNumber, 100m);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Cards.AddAsync(card);
        await _dbContext.Transactions.AddAsync(transaction);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.PostTransactionsAsync();

        // Assert
        result.RecordsFailed.Should().Be(1);
        result.Errors.Should().ContainMatch("*expired*");
    }

    [Fact]
    public async Task PostTransactionsAsync_ShouldFailWhenAccountNotFound()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId, accountId: 999);
        var card = CreateTestCard(account.AccountId);
        
        // Create transaction with non-existent account ID
        var transaction = new Transaction
        {
            TransactionId = "TXN123456789012",
            AccountId = 1, // Different from account.AccountId (999)
            CardNumber = card.CardNumber,
            TransactionType = "PU",
            CategoryCode = 1,
            TransactionSource = "ONLINE",
            Description = "Test",
            Amount = 100m,
            MerchantName = "Test",
            TransactionDate = DateTime.UtcNow,
            ProcessedFlag = "N"
        };

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Cards.AddAsync(card);
        await _dbContext.Transactions.AddAsync(transaction);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.PostTransactionsAsync();

        // Assert
        result.RecordsFailed.Should().Be(1);
        result.Errors.Should().ContainMatch("*Account*not found*");
    }

    [Fact]
    public async Task PostTransactionsAsync_ShouldFailWhenAccountInactive()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId);
        account.ActiveStatus = "N";
        var card = CreateTestCard(account.AccountId);
        var transaction = CreateTestTransaction(account.AccountId, card.CardNumber, 100m);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Cards.AddAsync(card);
        await _dbContext.Transactions.AddAsync(transaction);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.PostTransactionsAsync();

        // Assert
        result.RecordsFailed.Should().Be(1);
        result.Errors.Should().ContainMatch("*Account*is not active*");
    }

    [Fact]
    public async Task PostTransactionsAsync_ShouldFailWhenExceedsCreditLimit()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId, balance: 4900m, limit: 5000m);
        var card = CreateTestCard(account.AccountId);
        var transaction = CreateTestTransaction(account.AccountId, card.CardNumber, 200m); // Would exceed limit

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Cards.AddAsync(card);
        await _dbContext.Transactions.AddAsync(transaction);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.PostTransactionsAsync();

        // Assert
        result.RecordsFailed.Should().Be(1);
        result.Errors.Should().ContainMatch("*exceed credit limit*");
    }

    [Fact]
    public async Task PostTransactionsAsync_ShouldProcessPaymentTransactions()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId, balance: 1000m);
        var card = CreateTestCard(account.AccountId);
        var payment = CreateTestTransaction(account.AccountId, card.CardNumber, -500m); // Negative = payment

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Cards.AddAsync(card);
        await _dbContext.Transactions.AddAsync(payment);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.PostTransactionsAsync();

        // Assert
        result.RecordsSucceeded.Should().Be(1);
        
        var updatedAccount = await _dbContext.Accounts.FirstAsync();
        updatedAccount.CurrentBalance.Should().Be(500m); // 1000 - 500
        updatedAccount.CurrentCycleCredit.Should().Be(500m);
    }

    [Fact]
    public async Task PostTransactionsAsync_ShouldSkipAlreadyProcessedTransactions()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId, balance: 500m);
        var card = CreateTestCard(account.AccountId);
        var processedTransaction = CreateTestTransaction(account.AccountId, card.CardNumber, 100m, processedFlag: "Y");

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Cards.AddAsync(card);
        await _dbContext.Transactions.AddAsync(processedTransaction);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.PostTransactionsAsync();

        // Assert
        result.RecordsProcessed.Should().Be(0); // No unprocessed transactions
        
        var updatedAccount = await _dbContext.Accounts.FirstAsync();
        updatedAccount.CurrentBalance.Should().Be(500m); // Unchanged
    }

    [Fact]
    public async Task PostTransactionsAsync_ShouldMarkTransactionAsProcessed()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId);
        var card = CreateTestCard(account.AccountId);
        var transaction = CreateTestTransaction(account.AccountId, card.CardNumber, 50m);

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Cards.AddAsync(card);
        await _dbContext.Transactions.AddAsync(transaction);
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.PostTransactionsAsync();

        // Assert
        var processedTransaction = await _dbContext.Transactions.FirstAsync();
        processedTransaction.ProcessedFlag.Should().Be("Y");
    }

    [Fact]
    public async Task PostTransactionsAsync_ShouldReturnCorrectRecordCounts()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var account = CreateTestAccount(customer.CustomerId, balance: 500m, limit: 5000m);
        var card = CreateTestCard(account.AccountId);
        
        // Add multiple transactions
        var transaction1 = CreateTestTransaction(account.AccountId, card.CardNumber, 100m);
        transaction1.TransactionId = "TXN1111111111111";
        var transaction2 = CreateTestTransaction(account.AccountId, card.CardNumber, 200m);
        transaction2.TransactionId = "TXN2222222222222";
        var invalidTransaction = CreateTestTransaction(account.AccountId, "INVALID", 50m);
        invalidTransaction.TransactionId = "TXN3333333333333";

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Cards.AddAsync(card);
        await _dbContext.Transactions.AddRangeAsync(transaction1, transaction2, invalidTransaction);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.PostTransactionsAsync();

        // Assert
        result.RecordsProcessed.Should().Be(3);
        result.RecordsSucceeded.Should().Be(2);
        result.RecordsFailed.Should().Be(1);
    }
}

/// <summary>
/// Test DbContext that implements ICardDemoDbContext for unit testing
/// </summary>
public class CardDemoTestDbContext : DbContext, ICardDemoDbContext
{
    public CardDemoTestDbContext(DbContextOptions<CardDemoTestDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Card> Cards { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<TransactionCategory> TransactionCategories { get; set; } = null!;
    public DbSet<TransactionType> TransactionTypes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasKey(c => c.CustomerId);
        modelBuilder.Entity<Account>().HasKey(a => a.AccountId);
        modelBuilder.Entity<Card>().HasKey(c => c.CardNumber);
        modelBuilder.Entity<Transaction>().HasKey(t => t.TransactionId);
        modelBuilder.Entity<User>().HasKey(u => u.UserId);
        modelBuilder.Entity<TransactionCategory>().HasKey(tc => tc.CategoryCode);
        modelBuilder.Entity<TransactionType>().HasKey(tt => tt.TypeCode);

        // Configure Transaction relationships as optional for in-memory testing
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .IsRequired(false);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Card)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CardNumber)
            .HasPrincipalKey(c => c.CardNumber)
            .IsRequired(false);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.TransactionTypeNavigation)
            .WithMany(tt => tt.Transactions)
            .HasForeignKey(t => t.TransactionType)
            .HasPrincipalKey(tt => tt.TypeCode)
            .IsRequired(false);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany(tc => tc.Transactions)
            .HasForeignKey(t => t.CategoryCode)
            .IsRequired(false);

        // Ignore computed properties
        modelBuilder.Entity<Transaction>().Ignore(t => t.IsProcessed);
        modelBuilder.Entity<Transaction>().Ignore(t => t.IsDebit);
        modelBuilder.Entity<Transaction>().Ignore(t => t.IsCredit);
        modelBuilder.Entity<Transaction>().Ignore(t => t.IsReversal);
    }
}
