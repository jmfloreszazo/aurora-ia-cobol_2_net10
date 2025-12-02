using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Transactions.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace CardDemo.Tests.Unit.Application.Transactions;

public class GetAllTransactionsQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;
    private readonly GetAllTransactionsQueryHandler _handler;

    public GetAllTransactionsQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
        _handler = new GetAllTransactionsQueryHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ReturnsPagedTransactions_WhenTransactionsExist()
    {
        // Arrange
        var transactionType = new TransactionType { TypeCode = "01", TypeDescription = "Purchase" };
        var category = new TransactionCategory { CategoryCode = 1, CategoryDescription = "Retail" };

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
                Description = "Test Transaction 1",
                Amount = 100.00m,
                TransactionDate = DateTime.UtcNow.AddDays(-1),
                ProcessedFlag = "Y",
                TransactionTypeNavigation = transactionType,
                Category = category
            },
            new Transaction
            {
                TransactionId = "0000000000000002",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "01",
                CategoryCode = 1,
                TransactionSource = "POS",
                Description = "Test Transaction 2",
                Amount = 200.00m,
                TransactionDate = DateTime.UtcNow,
                ProcessedFlag = "N",
                TransactionTypeNavigation = transactionType,
                Category = category
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetAllTransactionsQuery(PageNumber: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectMaskedCardNumber()
    {
        // Arrange
        var transactionType = new TransactionType { TypeCode = "01", TypeDescription = "Purchase" };
        var category = new TransactionCategory { CategoryCode = 1, CategoryDescription = "Retail" };

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
                Description = "Test Transaction",
                Amount = 100.00m,
                TransactionDate = DateTime.UtcNow,
                ProcessedFlag = "Y",
                TransactionTypeNavigation = transactionType,
                Category = category
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetAllTransactionsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.First().MaskedCardNumber.Should().Be("**** **** **** 1111");
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoTransactions()
    {
        // Arrange
        var transactions = new List<Transaction>();
        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetAllTransactionsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ReturnsPaginatedResults_WhenPageSizeIsSmall()
    {
        // Arrange
        var transactionType = new TransactionType { TypeCode = "01", TypeDescription = "Purchase" };
        var category = new TransactionCategory { CategoryCode = 1, CategoryDescription = "Retail" };

        var transactions = Enumerable.Range(1, 25).Select(i => new Transaction
        {
            TransactionId = i.ToString("D16"),
            AccountId = 1000000001,
            CardNumber = "4111111111111111",
            TransactionType = "01",
            CategoryCode = 1,
            TransactionSource = "POS",
            Description = $"Transaction {i}",
            Amount = i * 10m,
            TransactionDate = DateTime.UtcNow.AddMinutes(-i),
            ProcessedFlag = "Y",
            TransactionTypeNavigation = transactionType,
            Category = category
        }).ToList();

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetAllTransactionsQuery(PageNumber: 2, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task Handle_SetsIsProcessedCorrectly()
    {
        // Arrange
        var transactionType = new TransactionType { TypeCode = "01", TypeDescription = "Purchase" };
        var category = new TransactionCategory { CategoryCode = 1, CategoryDescription = "Retail" };

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
                Description = "Processed",
                Amount = 100.00m,
                TransactionDate = DateTime.UtcNow,
                ProcessedFlag = "Y",
                TransactionTypeNavigation = transactionType,
                Category = category
            },
            new Transaction
            {
                TransactionId = "0000000000000002",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "01",
                CategoryCode = 1,
                TransactionSource = "POS",
                Description = "Not Processed",
                Amount = 200.00m,
                TransactionDate = DateTime.UtcNow.AddMinutes(-1),
                ProcessedFlag = "N",
                TransactionTypeNavigation = transactionType,
                Category = category
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetAllTransactionsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var processedTx = result.Items.First(t => t.ProcessedFlag == "Y");
        var notProcessedTx = result.Items.First(t => t.ProcessedFlag == "N");
        
        processedTx.IsProcessed.Should().BeTrue();
        notProcessedTx.IsProcessed.Should().BeFalse();
    }
}
