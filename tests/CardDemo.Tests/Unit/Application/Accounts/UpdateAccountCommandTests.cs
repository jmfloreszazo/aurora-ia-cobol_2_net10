using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Accounts.Commands;
using CardDemo.Application.Features.Accounts.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Accounts;

public class UpdateAccountCommandTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;

    public UpdateAccountCommandTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldUpdateCreditLimit_WhenValidRequest()
    {
        // Arrange
        var customer = new Customer { CustomerId = 1, FirstName = "John", LastName = "Doe" };
        var account = new Account
        {
            AccountId = 1,
            CustomerId = 1,
            Customer = customer,
            ActiveStatus = "Y",
            CurrentBalance = 1000m,
            CreditLimit = 5000m,
            CashCreditLimit = 1000m,
            OpenDate = DateTime.UtcNow.AddYears(-1),
            ExpirationDate = DateTime.UtcNow.AddYears(2)
        };

        var accounts = new List<Account> { account };
        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateAccountCommandHandler(_mockContext.Object);
        var command = new UpdateAccountCommand(1, null, 10000m, null, null, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CreditLimit.Should().Be(10000m);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateActiveStatus_WhenValidRequest()
    {
        // Arrange
        var customer = new Customer { CustomerId = 1, FirstName = "John", LastName = "Doe" };
        var account = new Account
        {
            AccountId = 1,
            CustomerId = 1,
            Customer = customer,
            ActiveStatus = "Y",
            CurrentBalance = 1000m,
            CreditLimit = 5000m,
            CashCreditLimit = 1000m,
            OpenDate = DateTime.UtcNow.AddYears(-1),
            ExpirationDate = DateTime.UtcNow.AddYears(2)
        };

        var accounts = new List<Account> { account };
        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateAccountCommandHandler(_mockContext.Object);
        var command = new UpdateAccountCommand(1, "N", null, null, null, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ActiveStatus.Should().Be("N");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenAccountNotFound()
    {
        // Arrange
        var accounts = new List<Account>();
        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

        var handler = new UpdateAccountCommandHandler(_mockContext.Object);
        var command = new UpdateAccountCommand(999, null, 10000m, null, null, null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldUpdateMultipleFields_WhenAllProvided()
    {
        // Arrange
        var customer = new Customer { CustomerId = 1, FirstName = "John", LastName = "Doe" };
        var account = new Account
        {
            AccountId = 1,
            CustomerId = 1,
            Customer = customer,
            ActiveStatus = "Y",
            CurrentBalance = 1000m,
            CreditLimit = 5000m,
            CashCreditLimit = 1000m,
            OpenDate = DateTime.UtcNow.AddYears(-1),
            ExpirationDate = DateTime.UtcNow.AddYears(2)
        };

        var accounts = new List<Account> { account };
        var mockDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var newExpiration = DateTime.UtcNow.AddYears(5);
        var handler = new UpdateAccountCommandHandler(_mockContext.Object);
        var command = new UpdateAccountCommand(1, "N", 15000m, 3000m, newExpiration, "NEW");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CreditLimit.Should().Be(15000m);
        result.CashCreditLimit.Should().Be(3000m);
        result.ActiveStatus.Should().Be("N");
    }
}
