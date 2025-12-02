using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Auth.Commands;
using CardDemo.Domain.Entities;
using CardDemo.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Auth;

public class RegisterCommandTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<DbSet<User>> _mockUserDbSet;

    public RegisterCommandTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockUserDbSet = new Mock<DbSet<User>>();
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenUserDoesNotExist()
    {
        // Arrange
        var users = new List<User>();
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        _mockPasswordHasher.Setup(p => p.HashPassword("password123")).Returns("hashed_password_123");

        var handler = new RegisterCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new RegisterCommand("newuser", "password123", "John", "Doe", "USER");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be("newuser");
        result.Message.Should().Be("User registered successfully");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenUserAlreadyExists()
    {
        // Arrange
        var existingUser = new User
        {
            UserId = "existinguser",
            PasswordHash = "hashed",
            FirstName = "Existing",
            LastName = "User",
            UserType = UserRole.USER,
            IsActive = true
        };

        var users = new List<User> { existingUser };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new RegisterCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new RegisterCommand("existinguser", "password123", "John", "Doe", "USER");

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User existinguser already exists");
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenUserTypeIsInvalid()
    {
        // Arrange
        var users = new List<User>();
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new RegisterCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new RegisterCommand("newuser", "password123", "John", "Doe", "InvalidType");

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid user type: InvalidType");
    }

    [Fact]
    public async Task Handle_ShouldHashPassword_WhenCreatingUser()
    {
        // Arrange
        var users = new List<User>();
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        _mockPasswordHasher.Setup(p => p.HashPassword("mypassword")).Returns("secure_hashed_password");

        var handler = new RegisterCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new RegisterCommand("newuser", "mypassword", "Jane", "Smith", "USER");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockPasswordHasher.Verify(p => p.HashPassword("mypassword"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateUserWithCorrectProperties()
    {
        // Arrange
        User? capturedUser = null;
        var users = new List<User>();
        var mockDbSet = users.BuildMockDbSet();
        mockDbSet.Setup(d => d.Add(It.IsAny<User>())).Callback<User>(u => capturedUser = u);
        
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        _mockPasswordHasher.Setup(p => p.HashPassword("password123")).Returns("hashed_password");

        var handler = new RegisterCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new RegisterCommand("newuser", "password123", "John", "Doe", "USER");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.UserId.Should().Be("newuser");
        capturedUser.PasswordHash.Should().Be("hashed_password");
        capturedUser.FirstName.Should().Be("John");
        capturedUser.LastName.Should().Be("Doe");
        capturedUser.UserType.Should().Be(UserRole.USER);
        capturedUser.IsActive.Should().BeTrue();
        capturedUser.IsLocked.Should().BeFalse();
        capturedUser.FailedLoginAttempts.Should().Be(0);
        capturedUser.CreatedBy.Should().Be("SYSTEM");
        capturedUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldCreateAdminUser_WhenUserTypeIsAdmin()
    {
        // Arrange
        User? capturedUser = null;
        var users = new List<User>();
        var mockDbSet = users.BuildMockDbSet();
        mockDbSet.Setup(d => d.Add(It.IsAny<User>())).Callback<User>(u => capturedUser = u);
        
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        _mockPasswordHasher.Setup(p => p.HashPassword("adminpass")).Returns("admin_hashed");

        var handler = new RegisterCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new RegisterCommand("adminuser", "adminpass", "Admin", "User", "ADMIN");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.UserType.Should().Be(UserRole.ADMIN);
    }

    [Fact]
    public async Task Handle_ShouldRejectInvalidCaseUserType()
    {
        // Arrange
        var users = new List<User>();
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new RegisterCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new RegisterCommand("newuser", "password123", "John", "Doe", "user"); // lowercase

        // Act - lowercase "user" should fail since Enum.TryParse is case-sensitive by default
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert - Should throw ArgumentException because Enum.TryParse without ignoreCase is case-sensitive
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid user type: user");
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChanges_WhenUserExists()
    {
        // Arrange
        var existingUser = new User
        {
            UserId = "existinguser",
            PasswordHash = "hashed",
            FirstName = "Existing",
            LastName = "User"
        };

        var users = new List<User> { existingUser };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new RegisterCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new RegisterCommand("existinguser", "password123", "John", "Doe", "USER");

        // Act
        try
        {
            await handler.Handle(command, CancellationToken.None);
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotHashPassword_WhenUserExists()
    {
        // Arrange
        var existingUser = new User
        {
            UserId = "existinguser",
            PasswordHash = "hashed",
            FirstName = "Existing",
            LastName = "User"
        };

        var users = new List<User> { existingUser };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new RegisterCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new RegisterCommand("existinguser", "password123", "John", "Doe", "USER");

        // Act
        try
        {
            await handler.Handle(command, CancellationToken.None);
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Assert
        _mockPasswordHasher.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
    }
}
