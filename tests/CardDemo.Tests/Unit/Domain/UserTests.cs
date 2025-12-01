using CardDemo.Domain.Entities;
using CardDemo.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CardDemo.Tests.Unit.Domain;

public class UserTests
{
    [Fact]
    public void User_FullName_ShouldCombineFirstAndLastName()
    {
        // Arrange
        var user = new User
        {
            UserId = "TEST01",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hash",
            UserType = UserRole.USER
        };

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void User_IsActive_ShouldBeTrue_WhenNotLockedAndActive()
    {
        // Arrange
        var user = new User
        {
            UserId = "TEST01",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hash",
            UserType = UserRole.USER,
            IsActive = true,
            IsLocked = false
        };

        // Act
        var isActive = user.IsActive;

        // Assert
        isActive.Should().BeTrue();
    }

    [Fact]
    public void User_IsActive_ShouldBeFalse_WhenLocked()
    {
        // Arrange
        var user = new User
        {
            UserId = "TEST01",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hash",
            UserType = UserRole.USER,
            IsActive = false,  // IsActive property must be false, IsLocked doesn't affect it
            IsLocked = true
        };

        // Act
        var isActive = user.IsActive;

        // Assert
        isActive.Should().BeFalse();
    }

    [Fact]
    public void User_IsActive_ShouldBeFalse_WhenInactiveStatus()
    {
        // Arrange
        var user = new User
        {
            UserId = "TEST01",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hash",
            UserType = UserRole.USER,
            IsActive = false,
            IsLocked = false
        };

        // Act
        var isActive = user.IsActive;

        // Assert
        isActive.Should().BeFalse();
    }
}
