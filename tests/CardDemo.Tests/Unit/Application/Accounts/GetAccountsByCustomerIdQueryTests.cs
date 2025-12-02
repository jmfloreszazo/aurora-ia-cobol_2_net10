using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Accounts.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Accounts;

public class GetAccountsByCustomerIdQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;

    public GetAccountsByCustomerIdQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldReturnAccounts_WhenCustomerHasAccounts()
    {
        // Arrange
        var customer = new Customer 
        { 
            CustomerId = 1, 
            FirstName = "John", 
            LastName = "Doe" 
        };
        
        var accounts = new List<Account>
        {
            new Account 
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
                GroupId = "GRP01",
                Cards = new List<Card>()
            },
            new Account 
            { 
                AccountId = 2, 
                CustomerId = 1, 
                Customer = customer, 
                ActiveStatus = "Y", 
                CurrentBalance = 2000.00m, 
                CreditLimit = 10000.00m,
                CashCreditLimit = 2000.00m,
                OpenDate = new DateTime(2021, 6, 1),
                ExpirationDate = new DateTime(2026, 6, 1),
                GroupId = "GRP02",
                Cards = new List<Card>()
            }
        };

        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

        var handler = new GetAccountsByCustomerIdQueryHandler(_mockContext.Object);
        var query = new GetAccountsByCustomerIdQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(a => a.CustomerId.Should().Be(1));
        result.Should().AllSatisfy(a => a.CustomerName.Should().Be("John Doe"));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenCustomerHasNoAccounts()
    {
        // Arrange
        var accounts = new List<Account>();
        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

        var handler = new GetAccountsByCustomerIdQueryHandler(_mockContext.Object);
        var query = new GetAccountsByCustomerIdQuery(999);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyCustomersAccounts_WhenMultipleCustomersExist()
    {
        // Arrange
        var customer1 = new Customer { CustomerId = 1, FirstName = "John", LastName = "Doe" };
        var customer2 = new Customer { CustomerId = 2, FirstName = "Jane", LastName = "Smith" };
        
        var accounts = new List<Account>
        {
            new Account 
            { 
                AccountId = 1, 
                CustomerId = 1, 
                Customer = customer1, 
                ActiveStatus = "Y", 
                CurrentBalance = 1000.00m, 
                CreditLimit = 5000.00m,
                CashCreditLimit = 1000.00m,
                OpenDate = new DateTime(2020, 1, 1),
                ExpirationDate = new DateTime(2025, 12, 31),
                Cards = new List<Card>()
            },
            new Account 
            { 
                AccountId = 2, 
                CustomerId = 2, 
                Customer = customer2, 
                ActiveStatus = "Y", 
                CurrentBalance = 2000.00m, 
                CreditLimit = 10000.00m,
                CashCreditLimit = 2000.00m,
                OpenDate = new DateTime(2021, 6, 1),
                ExpirationDate = new DateTime(2026, 6, 1),
                Cards = new List<Card>()
            },
            new Account 
            { 
                AccountId = 3, 
                CustomerId = 1, 
                Customer = customer1, 
                ActiveStatus = "N", 
                CurrentBalance = 500.00m, 
                CreditLimit = 3000.00m,
                CashCreditLimit = 500.00m,
                OpenDate = new DateTime(2019, 3, 15),
                ExpirationDate = new DateTime(2024, 3, 15),
                Cards = new List<Card>()
            }
        };

        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

        var handler = new GetAccountsByCustomerIdQueryHandler(_mockContext.Object);
        var query = new GetAccountsByCustomerIdQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(a => a.CustomerId.Should().Be(1));
        result.Select(a => a.AccountId).Should().Contain(1);
        result.Select(a => a.AccountId).Should().Contain(3);
        result.Select(a => a.AccountId).Should().NotContain(2);
    }

    [Fact]
    public async Task Handle_ShouldIncludeAccountDetails_WhenAccountExists()
    {
        // Arrange
        var customer = new Customer 
        { 
            CustomerId = 1, 
            FirstName = "Alice", 
            LastName = "Johnson" 
        };
        
        var account = new Account 
        { 
            AccountId = 1, 
            CustomerId = 1, 
            Customer = customer, 
            ActiveStatus = "Y", 
            CurrentBalance = 1500.00m, 
            CreditLimit = 7500.00m,
            CashCreditLimit = 1500.00m,
            OpenDate = new DateTime(2020, 6, 15),
            ExpirationDate = new DateTime(2025, 6, 15),
            GroupId = "PREMIUM",
            Cards = new List<Card>()
        };

        var accounts = new List<Account> { account };
        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

        var handler = new GetAccountsByCustomerIdQueryHandler(_mockContext.Object);
        var query = new GetAccountsByCustomerIdQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        var dto = result.First();
        dto.AccountId.Should().Be(1);
        dto.CustomerId.Should().Be(1);
        dto.CustomerName.Should().Be("Alice Johnson");
        dto.ActiveStatus.Should().Be("Y");
        dto.CurrentBalance.Should().Be(1500.00m);
        dto.CreditLimit.Should().Be(7500.00m);
        dto.CashCreditLimit.Should().Be(1500.00m);
        dto.OpenDate.Should().Be(new DateTime(2020, 6, 15));
        dto.ExpirationDate.Should().Be(new DateTime(2025, 6, 15));
        dto.GroupId.Should().Be("PREMIUM");
    }

    [Fact]
    public async Task Handle_ShouldHandleNullGroupId()
    {
        // Arrange
        var customer = new Customer 
        { 
            CustomerId = 1, 
            FirstName = "Bob", 
            LastName = "Wilson" 
        };
        
        var account = new Account 
        { 
            AccountId = 1, 
            CustomerId = 1, 
            Customer = customer, 
            ActiveStatus = "Y", 
            CurrentBalance = 500.00m, 
            CreditLimit = 2500.00m,
            CashCreditLimit = 500.00m,
            OpenDate = new DateTime(2022, 1, 1),
            ExpirationDate = new DateTime(2027, 1, 1),
            GroupId = null,
            Cards = new List<Card>()
        };

        var accounts = new List<Account> { account };
        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

        var handler = new GetAccountsByCustomerIdQueryHandler(_mockContext.Object);
        var query = new GetAccountsByCustomerIdQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().GroupId.Should().BeEmpty();
    }
}
