using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Reports.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace CardDemo.Tests.Unit.Application.Reports;

public class GetMonthlyReportQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;
    private readonly GetMonthlyReportQueryHandler _handler;

    public GetMonthlyReportQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
        _handler = new GetMonthlyReportQueryHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ReturnsReportSummary_WhenTransactionsExist()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15);
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = "0000000000000001",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "01",
                CategoryCode = 1,
                TransactionSource = "POS",
                Description = "Transaction 1",
                Amount = 100.00m,
                TransactionDate = new DateTime(2024, 6, 10),
                ProcessedFlag = "Y"
            },
            new Transaction
            {
                TransactionId = "0000000000000002",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "01",
                CategoryCode = 1,
                TransactionSource = "POS",
                Description = "Transaction 2",
                Amount = 200.00m,
                TransactionDate = new DateTime(2024, 6, 20),
                ProcessedFlag = "Y"
            },
            new Transaction
            {
                TransactionId = "0000000000000003",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "01",
                CategoryCode = 1,
                TransactionSource = "POS",
                Description = "Transaction 3",
                Amount = 300.00m,
                TransactionDate = new DateTime(2024, 6, 25),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetMonthlyReportQuery(Year: 2024, Month: 6);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().TransactionCount.Should().Be(3);
        result.First().TotalAmount.Should().Be(600.00m);
        result.First().AvgAmount.Should().Be(200.00m);
    }

    [Fact]
    public async Task Handle_FiltersTransactionsByMonth()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = "0000000000000001",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "01",
                CategoryCode = 1,
                TransactionSource = "POS",
                Description = "June Transaction",
                Amount = 100.00m,
                TransactionDate = new DateTime(2024, 6, 15),
                ProcessedFlag = "Y"
            },
            new Transaction
            {
                TransactionId = "0000000000000002",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "01",
                CategoryCode = 1,
                TransactionSource = "POS",
                Description = "July Transaction",
                Amount = 200.00m,
                TransactionDate = new DateTime(2024, 7, 15),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetMonthlyReportQuery(Year: 2024, Month: 6);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().TotalAmount.Should().Be(100.00m);
    }

    [Fact]
    public async Task Handle_FiltersTransactionsByAccountId_WhenProvided()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = "0000000000000001",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "01",
                CategoryCode = 1,
                TransactionSource = "POS",
                Description = "Account 1",
                Amount = 100.00m,
                TransactionDate = new DateTime(2024, 6, 15),
                ProcessedFlag = "Y"
            },
            new Transaction
            {
                TransactionId = "0000000000000002",
                AccountId = 1000000002,
                CardNumber = "4222222222222222",
                TransactionType = "01",
                CategoryCode = 1,
                TransactionSource = "POS",
                Description = "Account 2",
                Amount = 200.00m,
                TransactionDate = new DateTime(2024, 6, 20),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetMonthlyReportQuery(Year: 2024, Month: 6, AccountId: 1000000001);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().TotalAmount.Should().Be(100.00m);
        result.First().TransactionCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoTransactions()
    {
        // Arrange
        var transactions = new List<Transaction>();
        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetMonthlyReportQuery(Year: 2024, Month: 6);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsCorrectMonthFormat()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = "0000000000000001",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "01",
                CategoryCode = 1,
                TransactionSource = "POS",
                Description = "Test",
                Amount = 100.00m,
                TransactionDate = new DateTime(2024, 3, 15),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetMonthlyReportQuery(Year: 2024, Month: 3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Month.Should().Be("2024-03");
        result.First().Year.Should().Be(2024);
    }
}
