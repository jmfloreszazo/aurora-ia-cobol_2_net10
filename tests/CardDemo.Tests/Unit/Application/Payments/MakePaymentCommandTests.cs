using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Payments.Commands;
using CardDemo.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace CardDemo.Tests.Unit.Application.Payments;

public class MakePaymentCommandTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;
    private readonly MakePaymentCommandHandler _handler;

    public MakePaymentCommandTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
        _handler = new MakePaymentCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_MakesPayment_WhenAccountAndCardExist()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account
            {
                AccountId = 1000000001,
                CurrentBalance = 500.00m,
                CreditLimit = 1000.00m,
                ActiveStatus = "Y"
            }
        };

        var cards = new List<Card>
        {
            new Card
            {
                CardNumber = "4111111111111111",
                AccountId = 1000000001,
                ActiveStatus = "Y"
            }
        };

        var transactions = new List<Transaction>();

        var mockAccountDbSet = accounts.BuildMockDbSet();
        var mockCardDbSet = cards.BuildMockDbSet();
        var mockTransactionDbSet = transactions.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardDbSet.Object);
        _mockContext.Setup(c => c.Transactions).Returns(mockTransactionDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new MakePaymentCommand(
            AccountId: 1000000001,
            Amount: 100.00m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(1000000001);
        result.Amount.Should().Be(100.00m);
        result.NewBalance.Should().Be(400.00m);
        result.ConfirmationNumber.Should().StartWith("PAY");
        result.TransactionId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_ThrowsKeyNotFoundException_WhenAccountNotFound()
    {
        // Arrange
        var accounts = new List<Account>();
        var mockAccountDbSet = accounts.BuildMockDbSet();
        _mockContext.Setup(c => c.Accounts).Returns(mockAccountDbSet.Object);

        var command = new MakePaymentCommand(
            AccountId: 9999999999,
            Amount: 100.00m);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*9999999999*");
    }

    [Fact]
    public async Task Handle_ThrowsInvalidOperationException_WhenNoCardForAccount()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account
            {
                AccountId = 1000000001,
                CurrentBalance = 500.00m,
                ActiveStatus = "Y"
            }
        };

        var cards = new List<Card>();

        var mockAccountDbSet = accounts.BuildMockDbSet();
        var mockCardDbSet = cards.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardDbSet.Object);

        var command = new MakePaymentCommand(
            AccountId: 1000000001,
            Amount: 100.00m);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No card found*");
    }

    [Fact]
    public async Task Handle_ThrowsInvalidOperationException_WhenZeroBalance()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account
            {
                AccountId = 1000000001,
                CurrentBalance = 0.00m,
                ActiveStatus = "Y"
            }
        };

        var cards = new List<Card>
        {
            new Card
            {
                CardNumber = "4111111111111111",
                AccountId = 1000000001,
                ActiveStatus = "Y"
            }
        };

        var mockAccountDbSet = accounts.BuildMockDbSet();
        var mockCardDbSet = cards.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardDbSet.Object);

        var command = new MakePaymentCommand(
            AccountId: 1000000001,
            Amount: 100.00m);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no balance*");
    }

    [Fact]
    public async Task Handle_PaysFullBalance_WhenAmountExceedsBalance()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account
            {
                AccountId = 1000000001,
                CurrentBalance = 200.00m,
                ActiveStatus = "Y"
            }
        };

        var cards = new List<Card>
        {
            new Card
            {
                CardNumber = "4111111111111111",
                AccountId = 1000000001,
                ActiveStatus = "Y"
            }
        };

        var transactions = new List<Transaction>();

        var mockAccountDbSet = accounts.BuildMockDbSet();
        var mockCardDbSet = cards.BuildMockDbSet();
        var mockTransactionDbSet = transactions.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardDbSet.Object);
        _mockContext.Setup(c => c.Transactions).Returns(mockTransactionDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new MakePaymentCommand(
            AccountId: 1000000001,
            Amount: 500.00m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Amount.Should().Be(200.00m);
        result.NewBalance.Should().Be(0.00m);
    }

    [Fact]
    public async Task Handle_UsesPaymentDate_WhenProvided()
    {
        // Arrange
        var paymentDate = new DateTime(2024, 6, 15, 10, 30, 0);

        var accounts = new List<Account>
        {
            new Account
            {
                AccountId = 1000000001,
                CurrentBalance = 500.00m,
                ActiveStatus = "Y"
            }
        };

        var cards = new List<Card>
        {
            new Card
            {
                CardNumber = "4111111111111111",
                AccountId = 1000000001,
                ActiveStatus = "Y"
            }
        };

        var transactions = new List<Transaction>();

        var mockAccountDbSet = accounts.BuildMockDbSet();
        var mockCardDbSet = cards.BuildMockDbSet();
        var mockTransactionDbSet = transactions.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardDbSet.Object);
        _mockContext.Setup(c => c.Transactions).Returns(mockTransactionDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new MakePaymentCommand(
            AccountId: 1000000001,
            Amount: 100.00m,
            PaymentDate: paymentDate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Timestamp.Should().Be(paymentDate);
    }

    [Fact]
    public async Task Handle_GeneratesIncrementalTransactionId()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account
            {
                AccountId = 1000000001,
                CurrentBalance = 500.00m,
                ActiveStatus = "Y"
            }
        };

        var cards = new List<Card>
        {
            new Card
            {
                CardNumber = "4111111111111111",
                AccountId = 1000000001,
                ActiveStatus = "Y"
            }
        };

        var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = "0000000000000100",
                AccountId = 1000000001,
                CardNumber = "4111111111111111",
                TransactionType = "01",
                CategoryCode = 1,
                TransactionSource = "POS",
                Description = "Previous",
                Amount = 50.00m,
                TransactionDate = DateTime.UtcNow.AddDays(-1),
                ProcessedFlag = "Y"
            }
        };

        var mockAccountDbSet = accounts.BuildMockDbSet();
        var mockCardDbSet = cards.BuildMockDbSet();
        var mockTransactionDbSet = transactions.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardDbSet.Object);
        _mockContext.Setup(c => c.Transactions).Returns(mockTransactionDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new MakePaymentCommand(
            AccountId: 1000000001,
            Amount: 100.00m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TransactionId.Should().Be("0000000000000101");
    }

    [Fact]
    public async Task Handle_AddsTransactionToContext()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account
            {
                AccountId = 1000000001,
                CurrentBalance = 500.00m,
                ActiveStatus = "Y"
            }
        };

        var cards = new List<Card>
        {
            new Card
            {
                CardNumber = "4111111111111111",
                AccountId = 1000000001,
                ActiveStatus = "Y"
            }
        };

        var transactions = new List<Transaction>();

        var mockAccountDbSet = accounts.BuildMockDbSet();
        var mockCardDbSet = cards.BuildMockDbSet();
        var mockTransactionDbSet = transactions.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardDbSet.Object);
        _mockContext.Setup(c => c.Transactions).Returns(mockTransactionDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new MakePaymentCommand(
            AccountId: 1000000001,
            Amount: 100.00m);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        mockTransactionDbSet.Verify(m => m.Add(It.Is<Transaction>(t => 
            t.AccountId == 1000000001 && 
            t.Amount == 100.00m && 
            t.TransactionType == "02" && 
            t.Description.Contains("BILL PAYMENT"))), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsSaveChanges()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account
            {
                AccountId = 1000000001,
                CurrentBalance = 500.00m,
                ActiveStatus = "Y"
            }
        };

        var cards = new List<Card>
        {
            new Card
            {
                CardNumber = "4111111111111111",
                AccountId = 1000000001,
                ActiveStatus = "Y"
            }
        };

        var transactions = new List<Transaction>();

        var mockAccountDbSet = accounts.BuildMockDbSet();
        var mockCardDbSet = cards.BuildMockDbSet();
        var mockTransactionDbSet = transactions.BuildMockDbSet();

        _mockContext.Setup(c => c.Accounts).Returns(mockAccountDbSet.Object);
        _mockContext.Setup(c => c.Cards).Returns(mockCardDbSet.Object);
        _mockContext.Setup(c => c.Transactions).Returns(mockTransactionDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new MakePaymentCommand(
            AccountId: 1000000001,
            Amount: 100.00m);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
