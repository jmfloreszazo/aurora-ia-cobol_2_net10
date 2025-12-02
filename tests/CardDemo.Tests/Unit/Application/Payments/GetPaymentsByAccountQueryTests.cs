using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Payments;
using CardDemo.Application.Features.Payments.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace CardDemo.Tests.Unit.Application.Payments;

public class GetPaymentsByAccountQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;
    private readonly GetPaymentsByAccountQueryHandler _handler;

    public GetPaymentsByAccountQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
        _handler = new GetPaymentsByAccountQueryHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ReturnsPayments_WhenPaymentsExist()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = "0000000000000001",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "02", // Bill Payment
                CategoryCode = 2,
                TransactionSource = "POS TERM",
                Description = "BILL PAYMENT - ONLINE",
                Amount = 100.00m,
                TransactionDate = DateTime.UtcNow.AddDays(-5),
                ProcessedFlag = "Y"
            },
            new Transaction
            {
                TransactionId = "0000000000000002",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "02", // Bill Payment
                CategoryCode = 2,
                TransactionSource = "POS TERM",
                Description = "BILL PAYMENT - ONLINE",
                Amount = 200.00m,
                TransactionDate = DateTime.UtcNow.AddDays(-2),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetPaymentsByAccountQuery(AccountId: 1000000001);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(p => p.AccountId.Should().Be(1000000001));
    }

    [Fact]
    public async Task Handle_ReturnsEmptyResult_WhenNoPayments()
    {
        // Arrange
        var transactions = new List<Transaction>();
        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetPaymentsByAccountQuery(AccountId: 1000000001);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_FiltersOnlyPaymentTransactions()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = "0000000000000001",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "02", // Bill Payment
                CategoryCode = 2,
                TransactionSource = "POS TERM",
                Description = "BILL PAYMENT",
                Amount = 100.00m,
                TransactionDate = DateTime.UtcNow,
                ProcessedFlag = "Y"
            },
            new Transaction
            {
                TransactionId = "0000000000000002",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "01", // Purchase (not a payment)
                CategoryCode = 1,
                TransactionSource = "POS",
                Description = "PURCHASE",
                Amount = 50.00m,
                TransactionDate = DateTime.UtcNow,
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetPaymentsByAccountQuery(AccountId: 1000000001);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Amount.Should().Be(100.00m);
    }

    [Fact]
    public async Task Handle_FiltersPaymentsByAccountId()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = "0000000000000001",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "02",
                CategoryCode = 2,
                TransactionSource = "POS TERM",
                Description = "Account 1 Payment",
                Amount = 100.00m,
                TransactionDate = DateTime.UtcNow,
                ProcessedFlag = "Y"
            },
            new Transaction
            {
                TransactionId = "0000000000000002",
                AccountId = 1000000002,
                CardNumber = "4222222222222222",
                TransactionType = "02",
                CategoryCode = 2,
                TransactionSource = "POS TERM",
                Description = "Account 2 Payment",
                Amount = 200.00m,
                TransactionDate = DateTime.UtcNow,
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetPaymentsByAccountQuery(AccountId: 1000000001);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Amount.Should().Be(100.00m);
    }

    [Fact]
    public async Task Handle_ReturnsPaginatedResults()
    {
        // Arrange
        var transactions = Enumerable.Range(1, 25).Select(i => new Transaction
        {
            TransactionId = i.ToString("D16"),
            AccountId = 1000000001,
            CardNumber = "4111111111111111",
            TransactionType = "02",
            CategoryCode = 2,
            TransactionSource = "POS TERM",
            Description = $"Payment {i}",
            Amount = i * 10m,
            TransactionDate = DateTime.UtcNow.AddMinutes(-i),
            ProcessedFlag = "Y"
        }).ToList();

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetPaymentsByAccountQuery(
            AccountId: 1000000001,
            PageNumber: 2,
            PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_GeneratesConfirmationNumber()
    {
        // Arrange
        var paymentDate = new DateTime(2024, 6, 15, 10, 30, 45);
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = "0000000000000001",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "02",
                CategoryCode = 2,
                TransactionSource = "POS TERM",
                Description = "BILL PAYMENT",
                Amount = 100.00m,
                TransactionDate = paymentDate,
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetPaymentsByAccountQuery(AccountId: 1000000001);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.First().ConfirmationNumber.Should().StartWith("PAY");
        result.Items.First().ConfirmationNumber.Should().Contain("20240615");
    }

    [Fact]
    public async Task Handle_OrdersPaymentsByDateDescending()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = "0000000000000001",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "02",
                CategoryCode = 2,
                TransactionSource = "POS TERM",
                Description = "Old Payment",
                Amount = 100.00m,
                TransactionDate = DateTime.UtcNow.AddDays(-10),
                ProcessedFlag = "Y"
            },
            new Transaction
            {
                TransactionId = "0000000000000002",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "02",
                CategoryCode = 2,
                TransactionSource = "POS TERM",
                Description = "New Payment",
                Amount = 200.00m,
                TransactionDate = DateTime.UtcNow.AddDays(-1),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetPaymentsByAccountQuery(AccountId: 1000000001);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.First().Amount.Should().Be(200.00m); // Most recent first
        result.Items.Last().Amount.Should().Be(100.00m);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectPaymentDto()
    {
        // Arrange
        var paymentDate = DateTime.UtcNow;
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = "0000000000000001",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "02",
                CategoryCode = 2,
                TransactionSource = "POS TERM",
                Description = "BILL PAYMENT",
                Amount = 150.00m,
                TransactionDate = paymentDate,
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetPaymentsByAccountQuery(AccountId: 1000000001);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var payment = result.Items.First();
        payment.TransactionId.Should().Be("0000000000000001");
        payment.AccountId.Should().Be(1000000001);
        payment.Amount.Should().Be(150.00m);
        payment.Timestamp.Should().Be(paymentDate);
        payment.NewBalance.Should().Be(0); // Historical balance not stored
    }
}
