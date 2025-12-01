using CardDemo.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CardDemo.Tests.Unit.Domain;

public class CardTests
{
    [Fact]
    public void Card_IsActive_ShouldBeTrue_WhenActiveStatusIsY()
    {
        // Arrange
        var card = new Card
        {
            CardNumber = "4111111111111111",
            AccountId = 1,
            CardType = "VISA",
            EmbossedName = "JOHN DOE",
            ExpirationDate = "12/2028",
            ActiveStatus = "Y"
        };

        // Act
        var isActive = card.IsActive;

        // Assert
        isActive.Should().BeTrue();
    }

    [Fact]
    public void Card_IsExpired_ShouldBeTrue_WhenExpirationDatePassed()
    {
        // Arrange
        var card = new Card
        {
            CardNumber = "4111111111111111",
            AccountId = 1,
            CardType = "VISA",
            EmbossedName = "JOHN DOE",
            ExpirationDate = "12/2020",
            ActiveStatus = "Y"
        };

        // Act
        var isExpired = card.IsExpired;

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void Card_IsExpired_ShouldBeFalse_WhenExpirationDateNotReached()
    {
        // Arrange
        var futureDate = DateTime.Now.AddYears(2);
        var card = new Card
        {
            CardNumber = "4111111111111111",
            AccountId = 1,
            CardType = "VISA",
            EmbossedName = "JOHN DOE",
            ExpirationDate = $"{futureDate.Month:00}/{futureDate:yyyy}",
            ActiveStatus = "Y"
        };

        // Act
        var isExpired = card.IsExpired;

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void Card_MaskedCardNumber_ShouldMaskMiddleDigits()
    {
        // Arrange
        var card = new Card
        {
            CardNumber = "4111111111111111",
            AccountId = 1,
            CardType = "VISA",
            EmbossedName = "JOHN DOE",
            ExpirationDate = "12/2028",
            ActiveStatus = "Y"
        };

        // Act
        var masked = card.MaskedCardNumber;

        // Assert
        masked.Should().Be("**** **** **** 1111");
    }

    [Fact]
    public void Card_MaskedCardNumber_ShouldHandleShortCardNumber()
    {
        // Arrange
        var card = new Card
        {
            CardNumber = "411111",
            AccountId = 1,
            CardType = "VISA",
            EmbossedName = "JOHN DOE",
            ExpirationDate = "12/2028",
            ActiveStatus = "Y"
        };

        // Act
        var masked = card.MaskedCardNumber;

        // Assert
        masked.Should().Be("**** **** **** 1111"); // Always formats if length >= 4
    }
}
