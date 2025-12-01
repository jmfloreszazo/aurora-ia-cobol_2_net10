using CardDemo.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CardDemo.Tests.Unit.Domain;

public class AccountTests
{
    [Fact]
    public void Account_IsActive_ShouldBeTrue_WhenActiveStatusIsY()
    {
        // Arrange
        var account = new Account
        {
            CustomerId = 1,
            ActiveStatus = "Y",
            CurrentBalance = 1000,
            CreditLimit = 5000,
            CashCreditLimit = 1000
        };

        // Act
        var isActive = account.IsActive;

        // Assert
        isActive.Should().BeTrue();
    }

    [Fact]
    public void Account_AvailableCredit_ShouldCalculateCorrectly()
    {
        // Arrange
        var account = new Account
        {
            CustomerId = 1,
            ActiveStatus = "Y",
            CurrentBalance = 2500,
            CreditLimit = 10000,
            CashCreditLimit = 2000
        };

        // Act
        var availableCredit = account.AvailableCredit;

        // Assert
        availableCredit.Should().Be(7500); // 10000 - 2500
    }

    [Fact]
    public void Account_CreditUtilization_ShouldCalculatePercentageCorrectly()
    {
        // Arrange
        var account = new Account
        {
            CustomerId = 1,
            ActiveStatus = "Y",
            CurrentBalance = 2500,
            CreditLimit = 10000,
            CashCreditLimit = 2000
        };

        // Act
        var utilization = account.CreditUtilization;

        // Assert
        utilization.Should().Be(25.0m); // (2500 / 10000) * 100
    }

    [Fact]
    public void Account_CreditUtilization_ShouldReturnZero_WhenCreditLimitIsZero()
    {
        // Arrange
        var account = new Account
        {
            CustomerId = 1,
            ActiveStatus = "Y",
            CurrentBalance = 2500,
            CreditLimit = 0,
            CashCreditLimit = 0
        };

        // Act
        var utilization = account.CreditUtilization;

        // Assert
        utilization.Should().Be(0);
    }
}
