using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Auth.Commands;
using CardDemo.Domain.Entities;
using CardDemo.Domain.Enums;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Auth;

public class LoginCommandTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;

    public LoginCommandTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
    }

    [Fact]
    public async Task Handle_ShouldReturnLoginResponse_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new User
        {
            UserId = "testuser",
            PasswordHash = "hashedpassword",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserRole.USER,
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 0,
            LastLoginAt = DateTime.UtcNow.AddDays(-1)
        };

        var users = new List<User> { user };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        _mockPasswordHasher.Setup(p => p.VerifyPassword("password123", "hashedpassword")).Returns(true);
        _mockJwtTokenGenerator.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt-token-123");

        var handler = new LoginCommandHandler(_mockContext.Object, _mockPasswordHasher.Object, _mockJwtTokenGenerator.Object);
        var command = new LoginCommand("testuser", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("jwt-token-123");
        result.UserId.Should().Be("testuser");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.UserType.Should().Be("USER");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
    {
        // Arrange
        var users = new List<User>();
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new LoginCommandHandler(_mockContext.Object, _mockPasswordHasher.Object, _mockJwtTokenGenerator.Object);
        var command = new LoginCommand("nonexistent", "password123");

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenAccountIsLocked()
    {
        // Arrange
        var user = new User
        {
            UserId = "lockeduser",
            PasswordHash = "hashedpassword",
            FirstName = "Jane",
            LastName = "Smith",
            UserType = UserRole.USER,
            IsActive = true,
            IsLocked = true,
            FailedLoginAttempts = 3
        };

        var users = new List<User> { user };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new LoginCommandHandler(_mockContext.Object, _mockPasswordHasher.Object, _mockJwtTokenGenerator.Object);
        var command = new LoginCommand("lockeduser", "password123");

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Account is locked. Please contact administrator.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenAccountIsInactive()
    {
        // Arrange
        var user = new User
        {
            UserId = "inactiveuser",
            PasswordHash = "hashedpassword",
            FirstName = "Bob",
            LastName = "Wilson",
            UserType = UserRole.USER,
            IsActive = false,
            IsLocked = false,
            FailedLoginAttempts = 0
        };

        var users = new List<User> { user };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new LoginCommandHandler(_mockContext.Object, _mockPasswordHasher.Object, _mockJwtTokenGenerator.Object);
        var command = new LoginCommand("inactiveuser", "password123");

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Account is inactive.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = new User
        {
            UserId = "testuser",
            PasswordHash = "hashedpassword",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserRole.USER,
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 0
        };

        var users = new List<User> { user };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        _mockPasswordHasher.Setup(p => p.VerifyPassword("wrongpassword", "hashedpassword")).Returns(false);

        var handler = new LoginCommandHandler(_mockContext.Object, _mockPasswordHasher.Object, _mockJwtTokenGenerator.Object);
        var command = new LoginCommand("testuser", "wrongpassword");

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task Handle_ShouldIncrementFailedLoginAttempts_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = new User
        {
            UserId = "testuser",
            PasswordHash = "hashedpassword",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserRole.USER,
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 1
        };

        var users = new List<User> { user };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        _mockPasswordHasher.Setup(p => p.VerifyPassword("wrongpassword", "hashedpassword")).Returns(false);

        var handler = new LoginCommandHandler(_mockContext.Object, _mockPasswordHasher.Object, _mockJwtTokenGenerator.Object);
        var command = new LoginCommand("testuser", "wrongpassword");

        // Act
        try
        {
            await handler.Handle(command, CancellationToken.None);
        }
        catch (UnauthorizedAccessException)
        {
            // Expected
        }

        // Assert
        user.FailedLoginAttempts.Should().Be(2);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLockAccount_WhenFailedLoginAttemptsReach3()
    {
        // Arrange
        var user = new User
        {
            UserId = "testuser",
            PasswordHash = "hashedpassword",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserRole.USER,
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 2
        };

        var users = new List<User> { user };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        _mockPasswordHasher.Setup(p => p.VerifyPassword("wrongpassword", "hashedpassword")).Returns(false);

        var handler = new LoginCommandHandler(_mockContext.Object, _mockPasswordHasher.Object, _mockJwtTokenGenerator.Object);
        var command = new LoginCommand("testuser", "wrongpassword");

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Account is locked. Please contact administrator.");
        user.IsLocked.Should().BeTrue();
        user.FailedLoginAttempts.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldResetFailedLoginAttempts_WhenLoginIsSuccessful()
    {
        // Arrange
        var user = new User
        {
            UserId = "testuser",
            PasswordHash = "hashedpassword",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserRole.USER,
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 2
        };

        var users = new List<User> { user };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        _mockPasswordHasher.Setup(p => p.VerifyPassword("correctpassword", "hashedpassword")).Returns(true);
        _mockJwtTokenGenerator.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt-token");

        var handler = new LoginCommandHandler(_mockContext.Object, _mockPasswordHasher.Object, _mockJwtTokenGenerator.Object);
        var command = new LoginCommand("testuser", "correctpassword");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        user.FailedLoginAttempts.Should().Be(0);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldUpdateLastLoginAt_WhenLoginIsSuccessful()
    {
        // Arrange
        var oldLoginTime = DateTime.UtcNow.AddDays(-5);
        var user = new User
        {
            UserId = "testuser",
            PasswordHash = "hashedpassword",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserRole.USER,
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 0,
            LastLoginAt = oldLoginTime
        };

        var users = new List<User> { user };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        _mockPasswordHasher.Setup(p => p.VerifyPassword("password123", "hashedpassword")).Returns(true);
        _mockJwtTokenGenerator.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt-token");

        var handler = new LoginCommandHandler(_mockContext.Object, _mockPasswordHasher.Object, _mockJwtTokenGenerator.Object);
        var command = new LoginCommand("testuser", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        user.LastLoginAt.Should().BeAfter(oldLoginTime);
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldWorkForAdminUserType()
    {
        // Arrange
        var user = new User
        {
            UserId = "admin",
            PasswordHash = "hashedpassword",
            FirstName = "Admin",
            LastName = "User",
            UserType = UserRole.ADMIN,
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 0
        };

        var users = new List<User> { user };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        _mockPasswordHasher.Setup(p => p.VerifyPassword("adminpass", "hashedpassword")).Returns(true);
        _mockJwtTokenGenerator.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("admin-jwt-token");

        var handler = new LoginCommandHandler(_mockContext.Object, _mockPasswordHasher.Object, _mockJwtTokenGenerator.Object);
        var command = new LoginCommand("admin", "adminpass");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserType.Should().Be("ADMIN");
    }
}
