using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Accounts.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Accounts;

public class GetAllAccountsQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;

    public GetAllAccountsQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedAccounts_WhenAccountsExist()
    {
        // Arrange
        var customer = new Customer { CustomerId = 1, FirstName = "John", LastName = "Doe" };
        var accounts = new List<Account>
        {
            new Account { AccountId = 1, CustomerId = 1, Customer = customer, ActiveStatus = "Y", CurrentBalance = 1000.00m, CreditLimit = 5000.00m },
            new Account { AccountId = 2, CustomerId = 1, Customer = customer, ActiveStatus = "Y", CurrentBalance = 2000.00m, CreditLimit = 10000.00m },
            new Account { AccountId = 3, CustomerId = 2, Customer = customer, ActiveStatus = "N", CurrentBalance = 500.00m, CreditLimit = 3000.00m }
        };

        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

        var handler = new GetAllAccountsQueryHandler(_mockContext.Object);
        var query = new GetAllAccountsQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoAccountsExist()
    {
        // Arrange
        var accounts = new List<Account>();
        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

        var handler = new GetAllAccountsQueryHandler(_mockContext.Object);
        var query = new GetAllAccountsQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldRespectPagination()
    {
        // Arrange
        var customer = new Customer { CustomerId = 1, FirstName = "John", LastName = "Doe" };
        var accounts = Enumerable.Range(1, 25).Select(i => new Account
        {
            AccountId = i,
            CustomerId = 1,
            Customer = customer,
            ActiveStatus = "Y",
            CurrentBalance = i * 100.00m,
            CreditLimit = i * 500.00m
        }).ToList();

        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

        var handler = new GetAllAccountsQueryHandler(_mockContext.Object);
        var query = new GetAllAccountsQuery(2, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(2);
    }
}
