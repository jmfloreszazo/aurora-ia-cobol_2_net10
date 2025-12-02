using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Reports.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace CardDemo.Tests.Unit.Application.Reports;

public class GetCustomReportQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;
    private readonly GetCustomReportQueryHandler _handler;

    public GetCustomReportQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
        _handler = new GetCustomReportQueryHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ReturnsReportSummary_WhenTransactionsExistInDateRange()
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
                Description = "Transaction 1",
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
                Description = "Transaction 2",
                Amount = 200.00m,
                TransactionDate = new DateTime(2024, 7, 15),
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
                TransactionDate = new DateTime(2024, 8, 15),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetCustomReportQuery(
            StartDate: new DateTime(2024, 6, 1),
            EndDate: new DateTime(2024, 8, 31));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Sum(r => r.TotalAmount).Should().Be(600.00m);
    }

    [Fact]
    public async Task Handle_FiltersTransactionsByDateRange()
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
                Description = "In Range",
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
                Description = "Before Range",
                Amount = 200.00m,
                TransactionDate = new DateTime(2024, 5, 15),
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
                Description = "After Range",
                Amount = 300.00m,
                TransactionDate = new DateTime(2024, 8, 15),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetCustomReportQuery(
            StartDate: new DateTime(2024, 6, 1),
            EndDate: new DateTime(2024, 6, 30));

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

        var query = new GetCustomReportQuery(
            StartDate: new DateTime(2024, 6, 1),
            EndDate: new DateTime(2024, 6, 30),
            AccountId: 1000000001);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().TotalAmount.Should().Be(100.00m);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoTransactionsInRange()
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
                TransactionDate = new DateTime(2024, 1, 15),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetCustomReportQuery(
            StartDate: new DateTime(2024, 6, 1),
            EndDate: new DateTime(2024, 6, 30));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_GroupsTransactionsByYearAndMonth()
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
                Description = "Dec 2023",
                Amount = 100.00m,
                TransactionDate = new DateTime(2023, 12, 15),
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
                Description = "Jan 2024",
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
                Description = "Feb 2024",
                Amount = 300.00m,
                TransactionDate = new DateTime(2024, 2, 15),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetCustomReportQuery(
            StartDate: new DateTime(2023, 12, 1),
            EndDate: new DateTime(2024, 2, 28));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(r => r.Month == "2023-12" && r.Year == 2023);
        result.Should().Contain(r => r.Month == "2024-01" && r.Year == 2024);
        result.Should().Contain(r => r.Month == "2024-02" && r.Year == 2024);
    }

    [Fact]
    public async Task Handle_OrdersResultsByYearAndMonth()
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
                Description = "Feb 2024",
                Amount = 100.00m,
                TransactionDate = new DateTime(2024, 2, 15),
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
                Description = "Dec 2023",
                Amount = 200.00m,
                TransactionDate = new DateTime(2023, 12, 15),
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
                Description = "Jan 2024",
                Amount = 300.00m,
                TransactionDate = new DateTime(2024, 1, 15),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetCustomReportQuery(
            StartDate: new DateTime(2023, 12, 1),
            EndDate: new DateTime(2024, 2, 28));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result[0].Month.Should().Be("2023-12");
        result[1].Month.Should().Be("2024-01");
        result[2].Month.Should().Be("2024-02");
    }

    [Fact]
    public async Task Handle_CalculatesCorrectAverageAmount()
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
                TransactionDate = new DateTime(2024, 6, 15),
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
                TransactionDate = new DateTime(2024, 6, 20),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetCustomReportQuery(
            StartDate: new DateTime(2024, 6, 1),
            EndDate: new DateTime(2024, 6, 30));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().TransactionCount.Should().Be(3);
        result.First().TotalAmount.Should().Be(600.00m);
        result.First().AvgAmount.Should().Be(200.00m);
    }
}
