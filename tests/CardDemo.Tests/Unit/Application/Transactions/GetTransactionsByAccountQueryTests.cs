using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Transactions.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Transactions;

public class GetTransactionsByAccountQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;

    public GetTransactionsByAccountQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedTransactions_WhenTransactionsExist()
    {
        // Arrange
        var category = new TransactionCategory { CategoryCode = 5999, CategoryDescription = "General Merchandise" };
        var transactionType = new TransactionType { TypeCode = "PU", TypeDescription = "Purchase" };
        
        var transactions = new List<Transaction>
        {
            new Transaction 
            { 
                TransactionId = "TXN001",
                AccountId = 1,
                CardNumber = "4111111111111111",
                TransactionType = "PU",
                TransactionTypeNavigation = transactionType,
                CategoryCode = 5999,
                Category = category,
                TransactionSource = "POS",
                Description = "Test Purchase 1",
                Amount = 100.00m,
                MerchantId = "MERCH001",
                MerchantName = "Test Merchant",
                MerchantCity = "Test City",
                TransactionDate = DateTime.Now.AddDays(-1),
                ProcessedFlag = "Y"
            },
            new Transaction 
            { 
                TransactionId = "TXN002",
                AccountId = 1,
                CardNumber = "4111111111111111",
                TransactionType = "PU",
                TransactionTypeNavigation = transactionType,
                CategoryCode = 5999,
                Category = category,
                TransactionSource = "ONLINE",
                Description = "Test Purchase 2",
                Amount = 250.00m,
                MerchantId = "MERCH002",
                MerchantName = "Online Store",
                MerchantCity = "Web City",
                TransactionDate = DateTime.Now,
                ProcessedFlag = "N"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var handler = new GetTransactionsByAccountQueryHandler(_mockContext.Object);
        var query = new GetTransactionsByAccountQuery(1, 1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTransactionsExist()
    {
        // Arrange
        var transactions = new List<Transaction>();
        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var handler = new GetTransactionsByAccountQueryHandler(_mockContext.Object);
        var query = new GetTransactionsByAccountQuery(999, 1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyAccountTransactions()
    {
        // Arrange
        var category = new TransactionCategory { CategoryCode = 5999, CategoryDescription = "General Merchandise" };
        var transactionType = new TransactionType { TypeCode = "PU", TypeDescription = "Purchase" };
        
        var transactions = new List<Transaction>
        {
            new Transaction 
            { 
                TransactionId = "TXN001",
                AccountId = 1,
                CardNumber = "4111111111111111",
                TransactionType = "PU",
                TransactionTypeNavigation = transactionType,
                CategoryCode = 5999,
                Category = category,
                TransactionSource = "POS",
                Description = "Account 1 Transaction",
                Amount = 100.00m,
                TransactionDate = DateTime.Now,
                ProcessedFlag = "Y"
            },
            new Transaction 
            { 
                TransactionId = "TXN002",
                AccountId = 2,
                CardNumber = "5500000000000004",
                TransactionType = "PU",
                TransactionTypeNavigation = transactionType,
                CategoryCode = 5999,
                Category = category,
                TransactionSource = "POS",
                Description = "Account 2 Transaction",
                Amount = 200.00m,
                TransactionDate = DateTime.Now,
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var handler = new GetTransactionsByAccountQueryHandler(_mockContext.Object);
        var query = new GetTransactionsByAccountQuery(1, 1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Description.Should().Be("Account 1 Transaction");
    }

    [Fact]
    public async Task Handle_ShouldRespectPagination()
    {
        // Arrange
        var category = new TransactionCategory { CategoryCode = 5999, CategoryDescription = "General Merchandise" };
        var transactionType = new TransactionType { TypeCode = "PU", TypeDescription = "Purchase" };
        
        var transactions = Enumerable.Range(1, 25).Select(i => new Transaction
        {
            TransactionId = $"TXN{i:D3}",
            AccountId = 1,
            CardNumber = "4111111111111111",
            TransactionType = "PU",
            TransactionTypeNavigation = transactionType,
            CategoryCode = 5999,
            Category = category,
            TransactionSource = "POS",
            Description = $"Transaction {i}",
            Amount = i * 10.00m,
            TransactionDate = DateTime.Now.AddDays(-i),
            ProcessedFlag = "Y"
        }).ToList();

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var handler = new GetTransactionsByAccountQueryHandler(_mockContext.Object);
        var query = new GetTransactionsByAccountQuery(1, 2, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldOrderByTransactionDateDescending()
    {
        // Arrange
        var category = new TransactionCategory { CategoryCode = 5999, CategoryDescription = "General Merchandise" };
        var transactionType = new TransactionType { TypeCode = "PU", TypeDescription = "Purchase" };
        
        var transactions = new List<Transaction>
        {
            new Transaction 
            { 
                TransactionId = "TXN001",
                AccountId = 1,
                CardNumber = "4111111111111111",
                TransactionType = "PU",
                TransactionTypeNavigation = transactionType,
                CategoryCode = 5999,
                Category = category,
                TransactionSource = "POS",
                Description = "Oldest",
                Amount = 100.00m,
                TransactionDate = DateTime.Now.AddDays(-10),
                ProcessedFlag = "Y"
            },
            new Transaction 
            { 
                TransactionId = "TXN002",
                AccountId = 1,
                CardNumber = "4111111111111111",
                TransactionType = "PU",
                TransactionTypeNavigation = transactionType,
                CategoryCode = 5999,
                Category = category,
                TransactionSource = "POS",
                Description = "Newest",
                Amount = 200.00m,
                TransactionDate = DateTime.Now,
                ProcessedFlag = "Y"
            },
            new Transaction 
            { 
                TransactionId = "TXN003",
                AccountId = 1,
                CardNumber = "4111111111111111",
                TransactionType = "PU",
                TransactionTypeNavigation = transactionType,
                CategoryCode = 5999,
                Category = category,
                TransactionSource = "POS",
                Description = "Middle",
                Amount = 150.00m,
                TransactionDate = DateTime.Now.AddDays(-5),
                ProcessedFlag = "Y"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var handler = new GetTransactionsByAccountQueryHandler(_mockContext.Object);
        var query = new GetTransactionsByAccountQuery(1, 1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.Items[0].Description.Should().Be("Newest");
        result.Items[1].Description.Should().Be("Middle");
        result.Items[2].Description.Should().Be("Oldest");
    }

    [Fact]
    public async Task Handle_ShouldMaskCardNumber()
    {
        // Arrange
        var category = new TransactionCategory { CategoryCode = 5999, CategoryDescription = "General Merchandise" };
        var transactionType = new TransactionType { TypeCode = "PU", TypeDescription = "Purchase" };
        
        var transaction = new Transaction 
        { 
            TransactionId = "TXN001",
            AccountId = 1,
            CardNumber = "4111111111111111",
            TransactionType = "PU",
            TransactionTypeNavigation = transactionType,
            CategoryCode = 5999,
            Category = category,
            TransactionSource = "POS",
            Description = "Test Transaction",
            Amount = 100.00m,
            TransactionDate = DateTime.Now,
            ProcessedFlag = "Y"
        };

        var transactions = new List<Transaction> { transaction };
        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var handler = new GetTransactionsByAccountQueryHandler(_mockContext.Object);
        var query = new GetTransactionsByAccountQuery(1, 1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().MaskedCardNumber.Should().Be("**** **** **** 1111");
        result.Items.First().CardNumber.Should().Be("4111111111111111");
    }

    [Fact]
    public async Task Handle_ShouldIncludeProcessedFlagInfo()
    {
        // Arrange
        var category = new TransactionCategory { CategoryCode = 5999, CategoryDescription = "General Merchandise" };
        var transactionType = new TransactionType { TypeCode = "PU", TypeDescription = "Purchase" };
        
        var transactions = new List<Transaction>
        {
            new Transaction 
            { 
                TransactionId = "TXN001",
                AccountId = 1,
                CardNumber = "4111111111111111",
                TransactionType = "PU",
                TransactionTypeNavigation = transactionType,
                CategoryCode = 5999,
                Category = category,
                TransactionSource = "POS",
                Description = "Processed Transaction",
                Amount = 100.00m,
                TransactionDate = DateTime.Now,
                ProcessedFlag = "Y"
            },
            new Transaction 
            { 
                TransactionId = "TXN002",
                AccountId = 1,
                CardNumber = "4111111111111111",
                TransactionType = "PU",
                TransactionTypeNavigation = transactionType,
                CategoryCode = 5999,
                Category = category,
                TransactionSource = "POS",
                Description = "Pending Transaction",
                Amount = 200.00m,
                TransactionDate = DateTime.Now,
                ProcessedFlag = "N"
            }
        };

        var mockDbSet = transactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var handler = new GetTransactionsByAccountQueryHandler(_mockContext.Object);
        var query = new GetTransactionsByAccountQuery(1, 1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(t => t.IsProcessed == true && t.ProcessedFlag == "Y");
        result.Items.Should().Contain(t => t.IsProcessed == false && t.ProcessedFlag == "N");
    }
}
