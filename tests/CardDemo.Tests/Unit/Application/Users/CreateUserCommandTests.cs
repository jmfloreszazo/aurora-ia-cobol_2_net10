using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Users.Commands;
using CardDemo.Domain.Entities;
using CardDemo.Domain.Enums;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Users;

public class CreateUserCommandTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;

    public CreateUserCommandTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockPasswordHasher.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashed_password");
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenValidRequest()
    {
        // Arrange
        var users = new List<User>();
        var mockDbSet = users.BuildMockDbSet();
        mockDbSet.Setup(d => d.Add(It.IsAny<User>())).Callback<User>(u => users.Add(u));
        
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateUserCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new CreateUserCommand("NEWUSER1", "John", "Doe", "USER", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be("NEWUSER1");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.UserType.Should().Be("USER");
        result.IsActive.Should().BeTrue();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateAdminUser_WhenUserTypeIsAdmin()
    {
        // Arrange
        var users = new List<User>();
        var mockDbSet = users.BuildMockDbSet();
        mockDbSet.Setup(d => d.Add(It.IsAny<User>())).Callback<User>(u => users.Add(u));
        
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateUserCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new CreateUserCommand("ADMIN001", "Admin", "User", "ADMIN", "securepass");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be("ADMIN001");
        result.UserType.Should().Be("ADMIN");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserIdAlreadyExists()
    {
        // Arrange
        var existingUser = new User
        {
            UserId = "EXISTING",
            FirstName = "Existing",
            LastName = "User",
            UserType = UserRole.USER,
            IsActive = true
        };

        var users = new List<User> { existingUser };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new CreateUserCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new CreateUserCommand("EXISTING", "New", "User", "USER", "password");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldHashPassword()
    {
        // Arrange
        var users = new List<User>();
        var mockDbSet = users.BuildMockDbSet();
        mockDbSet.Setup(d => d.Add(It.IsAny<User>())).Callback<User>(u => users.Add(u));
        
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateUserCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new CreateUserCommand("USER01", "Test", "User", "USER", "mypassword");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockPasswordHasher.Verify(p => p.HashPassword("mypassword"), Times.Once);
    }
}
