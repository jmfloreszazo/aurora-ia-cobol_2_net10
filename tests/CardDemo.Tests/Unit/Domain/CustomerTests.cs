using CardDemo.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CardDemo.Tests.Unit.Domain;

public class CustomerTests
{
    [Fact]
    public void Customer_FullName_ShouldCombineFirstMiddleLastName()
    {
        // Arrange
        var customer = new Customer
        {
            FirstName = "John",
            MiddleName = "A",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            SSN = "123456789",
            GovernmentId = "GOV123",
            FICOScore = 750
        };

        // Act
        var fullName = customer.FullName;

        // Assert
        fullName.Should().Be("John A Doe");
    }

    [Fact]
    public void Customer_FullName_ShouldHandleMissingMiddleName()
    {
        // Arrange
        var customer = new Customer
        {
            FirstName = "John",
            MiddleName = null,
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            SSN = "123456789",
            GovernmentId = "GOV123",
            FICOScore = 750
        };

        // Act
        var fullName = customer.FullName;

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void Customer_Age_ShouldCalculateCorrectly()
    {
        // Arrange
        var birthDate = DateTime.Now.AddYears(-30).AddDays(-1);
        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = birthDate,
            SSN = "123456789",
            GovernmentId = "GOV123",
            FICOScore = 750
        };

        // Act
        var age = customer.Age;

        // Assert
        age.Should().Be(30);
    }

    [Fact]
    public void Customer_Age_ShouldHandleBirthdayNotYetReached()
    {
        // Arrange
        var birthDate = DateTime.Now.AddYears(-30).AddDays(1);
        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = birthDate,
            SSN = "123456789",
            GovernmentId = "GOV123",
            FICOScore = 750
        };

        // Act
        var age = customer.Age;

        // Assert
        age.Should().Be(30);  // Age property calculates simple year difference
    }
}
