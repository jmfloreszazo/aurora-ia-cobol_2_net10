using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Accounts.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Accounts;

public class GetAccountByIdQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;

    public GetAccountByIdQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldReturnAccountDetail_WhenAccountExists()
    {
        // Arrange
        var customer = new Customer 
        { 
            CustomerId = 1, 
            FirstName = "John", 
            LastName = "Doe" 
        };
        
        var card = new Card
        {
            CardNumber = "4111111111111111",
            AccountId = 1,
            CardType = "VISA",
            EmbossedName = "JOHN DOE",
            ExpirationDate = "12/2025",
            ActiveStatus = "Y"
        };

        var transaction = new Transaction
        {
            TransactionId = "TXN001",
            AccountId = 1,
            CardNumber = "4111111111111111",
            Amount = 100.00m,
            TransactionDate = DateTime.Now,
            Description = "Test Transaction",
            Card = card
        };

        var account = new Account
        {
            AccountId = 1,
            CustomerId = 1,
            Customer = customer,
            ActiveStatus = "Y",
            CurrentBalance = 1000.00m,
            CreditLimit = 5000.00m,
            CashCreditLimit = 1000.00m,
            OpenDate = new DateTime(2020, 1, 1),
            ExpirationDate = new DateTime(2025, 12, 31),
            ZipCode = "12345",
            GroupId = "GRP01",
            Cards = new List<Card> { card },
            Transactions = new List<Transaction> { transaction }
        };

        var accounts = new List<Account> { account };
        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

        var handler = new GetAccountByIdQueryHandler(_mockContext.Object);
        var query = new GetAccountByIdQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(1);
        result.CustomerId.Should().Be(1);
        result.CustomerName.Should().Be("John Doe");
        result.ActiveStatus.Should().Be("Y");
        result.CurrentBalance.Should().Be(1000.00m);
        result.CreditLimit.Should().Be(5000.00m);
        result.Cards.Should().HaveCount(1);
        result.RecentTransactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenAccountNotFound()
    {
        // Arrange
        var accounts = new List<Account>();
        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

        var handler = new GetAccountByIdQueryHandler(_mockContext.Object);
        var query = new GetAccountByIdQuery(999);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Account 999 not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnAccountWithEmptyCards_WhenNoCardsExist()
    {
        // Arrange
        var customer = new Customer 
        { 
            CustomerId = 1, 
            FirstName = "Jane", 
            LastName = "Smith" 
        };

        var account = new Account
        {
            AccountId = 2,
            CustomerId = 1,
            Customer = customer,
            ActiveStatus = "Y",
            CurrentBalance = 500.00m,
            CreditLimit = 3000.00m,
            CashCreditLimit = 500.00m,
            OpenDate = new DateTime(2021, 6, 1),
            ExpirationDate = new DateTime(2026, 6, 1),
            ZipCode = "54321",
            Cards = new List<Card>(),
            Transactions = new List<Transaction>()
        };

        var accounts = new List<Account> { account };
        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

        var handler = new GetAccountByIdQueryHandler(_mockContext.Object);
        var query = new GetAccountByIdQuery(2);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(2);
        result.Cards.Should().BeEmpty();
        result.RecentTransactions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnTransactions_WhenAccountHasManyTransactions()
    {
        // Note: The handler uses .Include(a => a.Transactions.OrderByDescending(...).Take(10))
        // but MockQueryable doesn't support filtered includes, so we test with exactly 10 transactions.
        // In production with real EF Core, the Take(10) would correctly limit the results.
        
        // Arrange
        var customer = new Customer 
        { 
            CustomerId = 1, 
            FirstName = "John", 
            LastName = "Doe" 
        };

        var card = new Card
        {
            CardNumber = "4111111111111111",
            AccountId = 3,
            CardType = "VISA",
            ActiveStatus = "Y"
        };

        // Create exactly 10 transactions to match what the handler should return
        var transactions = Enumerable.Range(1, 10).Select(i => new Transaction
        {
            TransactionId = $"TXN{i:D3}",
            AccountId = 3,
            CardNumber = "4111111111111111",
            Amount = i * 10.00m,
            TransactionDate = DateTime.Now.AddDays(-i),
            Description = $"Transaction {i}",
            Card = card
        }).ToList();

        var account = new Account
        {
            AccountId = 3,
            CustomerId = 1,
            Customer = customer,
            ActiveStatus = "Y",
            CurrentBalance = 2000.00m,
            CreditLimit = 10000.00m,
            CashCreditLimit = 2000.00m,
            OpenDate = new DateTime(2019, 1, 1),
            ExpirationDate = new DateTime(2024, 12, 31),
            ZipCode = "67890",
            Cards = new List<Card> { card },
            Transactions = transactions
        };

        var accounts = new List<Account> { account };
        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

        var handler = new GetAccountByIdQueryHandler(_mockContext.Object);
        var query = new GetAccountByIdQuery(3);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(3);
        result.RecentTransactions.Should().HaveCount(10);
        result.RecentTransactions.Should().AllSatisfy(t => t.TransactionId.Should().StartWith("TXN"));
    }

    [Fact]
    public async Task Handle_ShouldReturnMultipleCards_WhenAccountHasMultipleCards()
    {
        // Arrange
        var customer = new Customer 
        { 
            CustomerId = 1, 
            FirstName = "Alice", 
            LastName = "Johnson" 
        };

        var cards = new List<Card>
        {
            new Card
            {
                CardNumber = "4111111111111111",
                AccountId = 4,
                CardType = "VISA",
                EmbossedName = "ALICE JOHNSON",
                ExpirationDate = "12/2025",
                ActiveStatus = "Y"
            },
            new Card
            {
                CardNumber = "5500000000000004",
                AccountId = 4,
                CardType = "MASTERCARD",
                EmbossedName = "ALICE JOHNSON",
                ExpirationDate = "6/2026",
                ActiveStatus = "Y"
            }
        };

        var account = new Account
        {
            AccountId = 4,
            CustomerId = 1,
            Customer = customer,
            ActiveStatus = "Y",
            CurrentBalance = 1500.00m,
            CreditLimit = 7500.00m,
            CashCreditLimit = 1500.00m,
            OpenDate = new DateTime(2020, 3, 15),
            ExpirationDate = new DateTime(2025, 3, 15),
            ZipCode = "11111",
            Cards = cards,
            Transactions = new List<Transaction>()
        };

        var accounts = new List<Account> { account };
        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

        var handler = new GetAccountByIdQueryHandler(_mockContext.Object);
        var query = new GetAccountByIdQuery(4);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(4);
        result.Cards.Should().HaveCount(2);
        result.Cards.Select(c => c.CardType).Should().Contain("VISA");
        result.Cards.Select(c => c.CardType).Should().Contain("MASTERCARD");
    }
}
