using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Transactions.Commands;
using CardDemo.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Transactions;

public class CreateTransactionCommandTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;
    private readonly Mock<DbSet<Transaction>> _mockTransactionDbSet;

    public CreateTransactionCommandTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
        _mockTransactionDbSet = new Mock<DbSet<Transaction>>();
    }

    [Fact]
    public async Task Handle_ShouldCreateTransaction_WhenAllDataIsValid()
    {
        // Arrange
        var account = new Account 
        { 
            AccountId = 1, 
            CustomerId = 1, 
            CurrentBalance = 1000.00m,
            CurrentCycleCredit = 0,
            CurrentCycleDebit = 0
        };
        
        var card = new Card 
        { 
            CardNumber = "4111111111111111", 
            AccountId = 1,
            CardType = "VISA"
        };

        var category = new TransactionCategory 
        { 
            CategoryCode = 5999, 
            CategoryDescription = "General Merchandise" 
        };

        var transactionType = new TransactionType 
        { 
            TypeCode = "PU", 
            TypeDescription = "Purchase",
            CategoryCode = 5999,
            Category = category
        };

        var accounts = new List<Account> { account };
        var cards = new List<Card> { card };
        var transactionTypes = new List<TransactionType> { transactionType };
        var transactions = new List<Transaction>();

        var mockAccountsDbSet = accounts.BuildMockDbSet();
        var mockCardsDbSet = cards.BuildMockDbSet();
        var mockTransactionTypesDbSet = transactionTypes.BuildMockDbSet();
        var mockTransactionsDbSet = transactions.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountsDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardsDbSet.Object);
        _mockContext.Setup(c => c.TransactionTypes).Returns(mockTransactionTypesDbSet.Object);
        _mockContext.Setup(c => c.Transactions).Returns(mockTransactionsDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateTransactionCommandHandler(_mockContext.Object);
        var command = new CreateTransactionCommand(
            AccountId: 1,
            CardNumber: "4111111111111111",
            TransactionType: "PU",
            CategoryCode: 5999,
            TransactionSource: "POS",
            Description: "Test Purchase",
            Amount: 100.00m,
            MerchantId: "MERCH001",
            MerchantName: "Test Merchant",
            MerchantCity: "Test City",
            MerchantZip: "12345",
            TransactionDate: DateTime.UtcNow
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(1);
        result.CardNumber.Should().Be("4111111111111111");
        result.Amount.Should().Be(100.00m);
        result.Description.Should().Be("Test Purchase");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenAccountNotFound()
    {
        // Arrange
        var accounts = new List<Account>();
        var mockAccountsDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockAccountsDbSet.Object);

        var handler = new CreateTransactionCommandHandler(_mockContext.Object);
        var command = new CreateTransactionCommand(
            AccountId: 999,
            CardNumber: "4111111111111111",
            TransactionType: "PU",
            CategoryCode: 5999,
            TransactionSource: "POS",
            Description: "Test Purchase",
            Amount: 100.00m,
            MerchantId: null,
            MerchantName: null,
            MerchantCity: null,
            MerchantZip: null,
            TransactionDate: null
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Account 999 not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenCardNotFound()
    {
        // Arrange
        var account = new Account { AccountId = 1, CustomerId = 1 };
        var accounts = new List<Account> { account };
        var cards = new List<Card>();

        var mockAccountsDbSet = accounts.BuildMockDbSet();
        var mockCardsDbSet = cards.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountsDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardsDbSet.Object);

        var handler = new CreateTransactionCommandHandler(_mockContext.Object);
        var command = new CreateTransactionCommand(
            AccountId: 1,
            CardNumber: "9999999999999999",
            TransactionType: "PU",
            CategoryCode: 5999,
            TransactionSource: "POS",
            Description: "Test Purchase",
            Amount: 100.00m,
            MerchantId: null,
            MerchantName: null,
            MerchantCity: null,
            MerchantZip: null,
            TransactionDate: null
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Card 9999999999999999 not found for account 1");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenCardBelongsToDifferentAccount()
    {
        // Arrange
        var account = new Account { AccountId = 1, CustomerId = 1 };
        var card = new Card { CardNumber = "4111111111111111", AccountId = 2 }; // Different account
        
        var accounts = new List<Account> { account };
        var cards = new List<Card> { card };

        var mockAccountsDbSet = accounts.BuildMockDbSet();
        var mockCardsDbSet = cards.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountsDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardsDbSet.Object);

        var handler = new CreateTransactionCommandHandler(_mockContext.Object);
        var command = new CreateTransactionCommand(
            AccountId: 1,
            CardNumber: "4111111111111111",
            TransactionType: "PU",
            CategoryCode: 5999,
            TransactionSource: "POS",
            Description: "Test Purchase",
            Amount: 100.00m,
            MerchantId: null,
            MerchantName: null,
            MerchantCity: null,
            MerchantZip: null,
            TransactionDate: null
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Card 4111111111111111 not found for account 1");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTransactionTypeNotFound()
    {
        // Arrange
        var account = new Account { AccountId = 1, CustomerId = 1 };
        var card = new Card { CardNumber = "4111111111111111", AccountId = 1 };
        
        var accounts = new List<Account> { account };
        var cards = new List<Card> { card };
        var transactionTypes = new List<TransactionType>();

        var mockAccountsDbSet = accounts.BuildMockDbSet();
        var mockCardsDbSet = cards.BuildMockDbSet();
        var mockTransactionTypesDbSet = transactionTypes.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountsDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardsDbSet.Object);
        _mockContext.Setup(c => c.TransactionTypes).Returns(mockTransactionTypesDbSet.Object);

        var handler = new CreateTransactionCommandHandler(_mockContext.Object);
        var command = new CreateTransactionCommand(
            AccountId: 1,
            CardNumber: "4111111111111111",
            TransactionType: "INVALID",
            CategoryCode: 5999,
            TransactionSource: "POS",
            Description: "Test Purchase",
            Amount: 100.00m,
            MerchantId: null,
            MerchantName: null,
            MerchantCity: null,
            MerchantZip: null,
            TransactionDate: null
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Transaction type INVALID not found");
    }

    [Fact]
    public async Task Handle_ShouldUpdateAccountBalance_WhenTransactionIsCredit()
    {
        // Arrange
        var account = new Account 
        { 
            AccountId = 1, 
            CustomerId = 1, 
            CurrentBalance = 1000.00m,
            CurrentCycleCredit = 0,
            CurrentCycleDebit = 0
        };
        
        var card = new Card 
        { 
            CardNumber = "4111111111111111", 
            AccountId = 1 
        };

        var category = new TransactionCategory 
        { 
            CategoryCode = 5999, 
            CategoryDescription = "General" 
        };

        var transactionType = new TransactionType 
        { 
            TypeCode = "CR", 
            TypeDescription = "Credit",
            CategoryCode = 5999,
            Category = category
        };

        var accounts = new List<Account> { account };
        var cards = new List<Card> { card };
        var transactionTypes = new List<TransactionType> { transactionType };
        var transactions = new List<Transaction>();

        var mockAccountsDbSet = accounts.BuildMockDbSet();
        var mockCardsDbSet = cards.BuildMockDbSet();
        var mockTransactionTypesDbSet = transactionTypes.BuildMockDbSet();
        var mockTransactionsDbSet = transactions.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountsDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardsDbSet.Object);
        _mockContext.Setup(c => c.TransactionTypes).Returns(mockTransactionTypesDbSet.Object);
        _mockContext.Setup(c => c.Transactions).Returns(mockTransactionsDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateTransactionCommandHandler(_mockContext.Object);
        var command = new CreateTransactionCommand(
            AccountId: 1,
            CardNumber: "4111111111111111",
            TransactionType: "CR",
            CategoryCode: 5999,
            TransactionSource: "PAYMENT",
            Description: "Payment Received",
            Amount: 500.00m,
            MerchantId: null,
            MerchantName: null,
            MerchantCity: null,
            MerchantZip: null,
            TransactionDate: null
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        account.CurrentBalance.Should().Be(1500.00m); // 1000 + 500
        account.CurrentCycleCredit.Should().Be(500.00m);
    }

    [Fact]
    public async Task Handle_ShouldUpdateAccountBalance_WhenTransactionIsDebit()
    {
        // Arrange
        var account = new Account 
        { 
            AccountId = 1, 
            CustomerId = 1, 
            CurrentBalance = 1000.00m,
            CurrentCycleCredit = 0,
            CurrentCycleDebit = 0
        };
        
        var card = new Card 
        { 
            CardNumber = "4111111111111111", 
            AccountId = 1 
        };

        var category = new TransactionCategory 
        { 
            CategoryCode = 5999, 
            CategoryDescription = "General" 
        };

        var transactionType = new TransactionType 
        { 
            TypeCode = "DB", 
            TypeDescription = "Debit",
            CategoryCode = 5999,
            Category = category
        };

        var accounts = new List<Account> { account };
        var cards = new List<Card> { card };
        var transactionTypes = new List<TransactionType> { transactionType };
        var transactions = new List<Transaction>();

        var mockAccountsDbSet = accounts.BuildMockDbSet();
        var mockCardsDbSet = cards.BuildMockDbSet();
        var mockTransactionTypesDbSet = transactionTypes.BuildMockDbSet();
        var mockTransactionsDbSet = transactions.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountsDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardsDbSet.Object);
        _mockContext.Setup(c => c.TransactionTypes).Returns(mockTransactionTypesDbSet.Object);
        _mockContext.Setup(c => c.Transactions).Returns(mockTransactionsDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateTransactionCommandHandler(_mockContext.Object);
        var command = new CreateTransactionCommand(
            AccountId: 1,
            CardNumber: "4111111111111111",
            TransactionType: "DB",
            CategoryCode: 5999,
            TransactionSource: "POS",
            Description: "Purchase",
            Amount: -200.00m,
            MerchantId: null,
            MerchantName: null,
            MerchantCity: null,
            MerchantZip: null,
            TransactionDate: null
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        account.CurrentBalance.Should().Be(800.00m); // 1000 - 200
        account.CurrentCycleDebit.Should().Be(200.00m);
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentUtcTime_WhenTransactionDateIsNull()
    {
        // Arrange
        var account = new Account 
        { 
            AccountId = 1, 
            CustomerId = 1, 
            CurrentBalance = 1000.00m 
        };
        
        var card = new Card 
        { 
            CardNumber = "4111111111111111", 
            AccountId = 1 
        };

        var category = new TransactionCategory 
        { 
            CategoryCode = 5999, 
            CategoryDescription = "General" 
        };

        var transactionType = new TransactionType 
        { 
            TypeCode = "PU", 
            TypeDescription = "Purchase",
            CategoryCode = 5999,
            Category = category
        };

        Transaction? capturedTransaction = null;
        var accounts = new List<Account> { account };
        var cards = new List<Card> { card };
        var transactionTypes = new List<TransactionType> { transactionType };
        var transactions = new List<Transaction>();

        var mockAccountsDbSet = accounts.BuildMockDbSet();
        var mockCardsDbSet = cards.BuildMockDbSet();
        var mockTransactionTypesDbSet = transactionTypes.BuildMockDbSet();
        var mockTransactionsDbSet = transactions.BuildMockDbSet();
        mockTransactionsDbSet.Setup(d => d.Add(It.IsAny<Transaction>())).Callback<Transaction>(t => capturedTransaction = t);

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountsDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardsDbSet.Object);
        _mockContext.Setup(c => c.TransactionTypes).Returns(mockTransactionTypesDbSet.Object);
        _mockContext.Setup(c => c.Transactions).Returns(mockTransactionsDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateTransactionCommandHandler(_mockContext.Object);
        var command = new CreateTransactionCommand(
            AccountId: 1,
            CardNumber: "4111111111111111",
            TransactionType: "PU",
            CategoryCode: 5999,
            TransactionSource: "POS",
            Description: "Test",
            Amount: 100.00m,
            MerchantId: null,
            MerchantName: null,
            MerchantCity: null,
            MerchantZip: null,
            TransactionDate: null
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedTransaction.Should().NotBeNull();
        capturedTransaction!.TransactionDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldSetProcessedFlagToN()
    {
        // Arrange
        var account = new Account 
        { 
            AccountId = 1, 
            CustomerId = 1, 
            CurrentBalance = 1000.00m 
        };
        
        var card = new Card 
        { 
            CardNumber = "4111111111111111", 
            AccountId = 1 
        };

        var category = new TransactionCategory 
        { 
            CategoryCode = 5999, 
            CategoryDescription = "General" 
        };

        var transactionType = new TransactionType 
        { 
            TypeCode = "PU", 
            TypeDescription = "Purchase",
            CategoryCode = 5999,
            Category = category
        };

        Transaction? capturedTransaction = null;
        var accounts = new List<Account> { account };
        var cards = new List<Card> { card };
        var transactionTypes = new List<TransactionType> { transactionType };
        var transactions = new List<Transaction>();

        var mockAccountsDbSet = accounts.BuildMockDbSet();
        var mockCardsDbSet = cards.BuildMockDbSet();
        var mockTransactionTypesDbSet = transactionTypes.BuildMockDbSet();
        var mockTransactionsDbSet = transactions.BuildMockDbSet();
        mockTransactionsDbSet.Setup(d => d.Add(It.IsAny<Transaction>())).Callback<Transaction>(t => capturedTransaction = t);

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountsDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardsDbSet.Object);
        _mockContext.Setup(c => c.TransactionTypes).Returns(mockTransactionTypesDbSet.Object);
        _mockContext.Setup(c => c.Transactions).Returns(mockTransactionsDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateTransactionCommandHandler(_mockContext.Object);
        var command = new CreateTransactionCommand(
            AccountId: 1,
            CardNumber: "4111111111111111",
            TransactionType: "PU",
            CategoryCode: 5999,
            TransactionSource: "POS",
            Description: "Test",
            Amount: 100.00m,
            MerchantId: null,
            MerchantName: null,
            MerchantCity: null,
            MerchantZip: null,
            TransactionDate: null
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedTransaction.Should().NotBeNull();
        capturedTransaction!.ProcessedFlag.Should().Be("N");
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueTransactionId()
    {
        // Arrange
        var account = new Account 
        { 
            AccountId = 1, 
            CustomerId = 1, 
            CurrentBalance = 1000.00m 
        };
        
        var card = new Card 
        { 
            CardNumber = "4111111111111111", 
            AccountId = 1 
        };

        var category = new TransactionCategory 
        { 
            CategoryCode = 5999, 
            CategoryDescription = "General" 
        };

        var transactionType = new TransactionType 
        { 
            TypeCode = "PU", 
            TypeDescription = "Purchase",
            CategoryCode = 5999,
            Category = category
        };

        var accounts = new List<Account> { account };
        var cards = new List<Card> { card };
        var transactionTypes = new List<TransactionType> { transactionType };
        var transactions = new List<Transaction>();

        var mockAccountsDbSet = accounts.BuildMockDbSet();
        var mockCardsDbSet = cards.BuildMockDbSet();
        var mockTransactionTypesDbSet = transactionTypes.BuildMockDbSet();
        var mockTransactionsDbSet = transactions.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountsDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardsDbSet.Object);
        _mockContext.Setup(c => c.TransactionTypes).Returns(mockTransactionTypesDbSet.Object);
        _mockContext.Setup(c => c.Transactions).Returns(mockTransactionsDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateTransactionCommandHandler(_mockContext.Object);
        var command = new CreateTransactionCommand(
            AccountId: 1,
            CardNumber: "4111111111111111",
            TransactionType: "PU",
            CategoryCode: 5999,
            TransactionSource: "POS",
            Description: "Test",
            Amount: 100.00m,
            MerchantId: null,
            MerchantName: null,
            MerchantCity: null,
            MerchantZip: null,
            TransactionDate: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.TransactionId.Should().NotBeNullOrEmpty();
        result.TransactionId.Should().StartWith("T");
    }
}
