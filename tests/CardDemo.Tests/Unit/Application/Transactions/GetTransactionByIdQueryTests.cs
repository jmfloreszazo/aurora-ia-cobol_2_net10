using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Transactions.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace CardDemo.Tests.Unit.Application.Transactions;

public class GetTransactionByIdQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;
    private readonly GetTransactionByIdQueryHandler _handler;

    public GetTransactionByIdQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
        _handler = new GetTransactionByIdQueryHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ReturnsTransaction_WhenFound()
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
                MerchantId = "123456789",
                MerchantName = "Test Merchant",
                MerchantCity = "New York",
                TransactionDate = DateTime.UtcNow,
                ProcessedFlag = "Y",
                TransactionTypeNavigation = transactionType,
                Category = category
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetTransactionByIdQuery("0000000000000001");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TransactionId.Should().Be("0000000000000001");
        result.AccountId.Should().Be(1000000001);
        result.Amount.Should().Be(100.00m);
        result.Description.Should().Be("Test Transaction");
        result.MerchantName.Should().Be("Test Merchant");
        result.IsProcessed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var transactions = new List<Transaction>();
        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetTransactionByIdQuery("9999999999999999");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ReturnsMaskedCardNumber()
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
                Description = "Test",
                Amount = 100.00m,
                TransactionDate = DateTime.UtcNow,
                ProcessedFlag = "Y",
                TransactionTypeNavigation = transactionType,
                Category = category
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetTransactionByIdQuery("0000000000000001");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.MaskedCardNumber.Should().Be("**** **** **** 1111");
        result.CardNumber.Should().Be("4111111111111111");
    }

    [Fact]
    public async Task Handle_ReturnsCorrectTransactionTypeDescription()
    {
        // Arrange
        var transactionType = new TransactionType { TypeCode = "02", TypeDescription = "Bill Payment" };
        var category = new TransactionCategory { CategoryCode = 2, CategoryDescription = "Payment" };

        var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = "0000000000000001",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "02",
                CategoryCode = 2,
                TransactionSource = "ONLINE",
                Description = "Bill Payment",
                Amount = 500.00m,
                TransactionDate = DateTime.UtcNow,
                ProcessedFlag = "Y",
                TransactionTypeNavigation = transactionType,
                Category = category
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetTransactionByIdQuery("0000000000000001");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TransactionType.Should().Be("02");
        result.TransactionTypeDescription.Should().Be("Bill Payment");
        result.CategoryDescription.Should().Be("Payment");
    }

    [Fact]
    public async Task Handle_ReturnsCorrectTransaction_WhenMultipleExist()
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
                Description = "First Transaction",
                Amount = 100.00m,
                TransactionDate = DateTime.UtcNow,
                ProcessedFlag = "Y",
                TransactionTypeNavigation = transactionType,
                Category = category
            },
            new Transaction
            {
                TransactionId = "0000000000000002",
                AccountId = 1000000002,
                CardNumber = "4222222222222222",
                TransactionType = "01",
                CategoryCode = 1,
                TransactionSource = "POS",
                Description = "Second Transaction",
                Amount = 200.00m,
                TransactionDate = DateTime.UtcNow,
                ProcessedFlag = "Y",
                TransactionTypeNavigation = transactionType,
                Category = category
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetTransactionByIdQuery("0000000000000002");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TransactionId.Should().Be("0000000000000002");
        result.Description.Should().Be("Second Transaction");
        result.Amount.Should().Be(200.00m);
    }
}
