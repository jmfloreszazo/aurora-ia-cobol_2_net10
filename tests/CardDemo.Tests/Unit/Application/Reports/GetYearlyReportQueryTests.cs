using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Reports.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace CardDemo.Tests.Unit.Application.Reports;

public class GetYearlyReportQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;
    private readonly GetYearlyReportQueryHandler _handler;

    public GetYearlyReportQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
        _handler = new GetYearlyReportQueryHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ReturnsYearlySummary_WhenTransactionsExist()
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
                Description = "January",
                Amount = 100.00m,
                TransactionDate = new DateTime(2024, 1, 15),
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
                Description = "February",
                Amount = 200.00m,
                TransactionDate = new DateTime(2024, 2, 15),
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
                Description = "March",
                Amount = 300.00m,
                TransactionDate = new DateTime(2024, 3, 15),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetYearlyReportQuery(Year: 2024);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Sum(r => r.TotalAmount).Should().Be(600.00m);
        result.Sum(r => r.TransactionCount).Should().Be(3);
    }

    [Fact]
    public async Task Handle_FiltersTransactionsByYear()
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
                Description = "2024 Transaction",
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
                Description = "2023 Transaction",
                Amount = 200.00m,
                TransactionDate = new DateTime(2023, 6, 15),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetYearlyReportQuery(Year: 2024);

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

        var query = new GetYearlyReportQuery(Year: 2024, AccountId: 1000000001);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().TotalAmount.Should().Be(100.00m);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoTransactions()
    {
        // Arrange
        var transactions = new List<Transaction>();
        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetYearlyReportQuery(Year: 2024);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_GroupsTransactionsByMonth()
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
                Description = "June 1",
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
                Description = "June 2",
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
                Description = "July",
                Amount = 300.00m,
                TransactionDate = new DateTime(2024, 7, 15),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetYearlyReportQuery(Year: 2024);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        
        var juneReport = result.First(r => r.Month == "2024-06");
        juneReport.TransactionCount.Should().Be(2);
        juneReport.TotalAmount.Should().Be(300.00m);
        juneReport.AvgAmount.Should().Be(150.00m);

        var julyReport = result.First(r => r.Month == "2024-07");
        julyReport.TransactionCount.Should().Be(1);
        julyReport.TotalAmount.Should().Be(300.00m);
    }

    [Fact]
    public async Task Handle_OrdersResultsByMonth()
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
                Description = "December",
                Amount = 100.00m,
                TransactionDate = new DateTime(2024, 12, 15),
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
                Description = "January",
                Amount = 200.00m,
                TransactionDate = new DateTime(2024, 1, 15),
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
                Description = "June",
                Amount = 300.00m,
                TransactionDate = new DateTime(2024, 6, 15),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetYearlyReportQuery(Year: 2024);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result[0].Month.Should().Be("2024-01");
        result[1].Month.Should().Be("2024-06");
        result[2].Month.Should().Be("2024-12");
    }
}
