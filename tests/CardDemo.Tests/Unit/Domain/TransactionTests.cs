using CardDemo.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CardDemo.Tests.Unit.Domain;

public class TransactionTests
{
    [Fact]
    public void Transaction_IsProcessed_ShouldBeTrue_WhenProcessedFlagIsY()
    {
        // Arrange
        var transaction = new Transaction
        {
            AccountId = 1,
            CardNumber = "4111111111111111",
            TransactionType = "PURCHASE",
            CategoryCode = 1,
            Description = "Test Purchase",
            Amount = 100.00m,
            TransactionDate = DateTime.Now,
            ProcessedFlag = "Y"
        };

        // Act
        var isProcessed = transaction.IsProcessed;

        // Assert
        isProcessed.Should().BeTrue();
    }

    [Fact]
    public void Transaction_IsDebit_ShouldBeTrue_ForPurchaseTransaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            AccountId = 1,
            CardNumber = "4111111111111111",
            TransactionType = "PURCHASE",
            CategoryCode = 1,
            Description = "Test Purchase",
            Amount = -100.00m,  // IsDebit checks if Amount < 0
            TransactionDate = DateTime.Now,
            ProcessedFlag = "N"
        };

        // Act
        var isDebit = transaction.IsDebit;

        // Assert
        isDebit.Should().BeTrue();
    }

    [Fact]
    public void Transaction_IsCredit_ShouldBeTrue_ForPaymentTransaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            AccountId = 1,
            CardNumber = "4111111111111111",
            TransactionType = "PAYMENT",
            CategoryCode = 1,
            Description = "Payment",
            Amount = 500.00m,
            TransactionDate = DateTime.Now,
            ProcessedFlag = "N"
        };

        // Act
        var isCredit = transaction.IsCredit;

        // Assert
        isCredit.Should().BeTrue();
    }

    [Fact]
    public void Transaction_IsCredit_ShouldBeTrue_ForRefundTransaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            AccountId = 1,
            CardNumber = "4111111111111111",
            TransactionType = "REFUND",
            CategoryCode = 1,
            Description = "Refund",
            Amount = 50.00m,
            TransactionDate = DateTime.Now,
            ProcessedFlag = "N"
        };

        // Act
        var isCredit = transaction.IsCredit;

        // Assert
        isCredit.Should().BeTrue();
    }

    [Fact]
    public void Transaction_IsReversal_ShouldBeTrue_WhenAmountIsNegative()
    {
        // Arrange
        var transaction = new Transaction
        {
            AccountId = 1,
            CardNumber = "4111111111111111",
            TransactionType = "REVERSAL",
            CategoryCode = 1,
            Description = "Reversal",
            Amount = -100.00m,
            OrigTransactionId = "TRX001",  // IsReversal checks if OrigTransactionId is not null/empty
            TransactionDate = DateTime.Now,
            ProcessedFlag = "N"
        };

        // Act
        var isReversal = transaction.IsReversal;

        // Assert
        isReversal.Should().BeTrue();
    }
}
