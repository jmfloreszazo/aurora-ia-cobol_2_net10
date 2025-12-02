using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Users.Commands;
using CardDemo.Domain.Entities;
using CardDemo.Domain.Enums;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Users;

public class UpdateUserCommandTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;

    public UpdateUserCommandTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockPasswordHasher.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("new_hashed_password");
    }

    [Fact]
    public async Task Handle_ShouldUpdateFirstName_WhenValidRequest()
    {
        // Arrange
        var user = new User
        {
            UserId = "USER001",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserRole.USER,
            IsActive = true
        };

        var users = new List<User> { user };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateUserCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new UpdateUserCommand("USER001", "Jonathan", null, null, null, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jonathan");
        result.LastName.Should().Be("Doe"); // Unchanged
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateLastName_WhenValidRequest()
    {
        // Arrange
        var user = new User
        {
            UserId = "USER001",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserRole.USER,
            IsActive = true
        };

        var users = new List<User> { user };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateUserCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new UpdateUserCommand("USER001", null, "Smith", null, null, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John"); // Unchanged
        result.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserType_WhenValidRequest()
    {
        // Arrange
        var user = new User
        {
            UserId = "USER001",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserRole.USER,
            IsActive = true
        };

        var users = new List<User> { user };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateUserCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new UpdateUserCommand("USER001", null, null, "ADMIN", null, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserType.Should().Be("ADMIN");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var users = new List<User>();
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new UpdateUserCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new UpdateUserCommand("NOTFOUND", "New", null, null, null, null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldUpdateMultipleFields_WhenAllProvided()
    {
        // Arrange
        var user = new User
        {
            UserId = "USER001",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserRole.USER,
            IsActive = true
        };

        var users = new List<User> { user };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateUserCommandHandler(_mockContext.Object, _mockPasswordHasher.Object);
        var command = new UpdateUserCommand("USER001", "Jane", "Smith", "ADMIN", "A", null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        result.UserType.Should().Be("ADMIN");
    }
}
