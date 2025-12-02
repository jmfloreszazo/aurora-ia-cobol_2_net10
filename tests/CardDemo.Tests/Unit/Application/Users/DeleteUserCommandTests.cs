using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Users.Commands;
using CardDemo.Domain.Entities;
using CardDemo.Domain.Enums;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Users;

public class DeleteUserCommandTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;

    public DeleteUserCommandTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldDeactivateUser_WhenUserExists()
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

        var handler = new DeleteUserCommandHandler(_mockContext.Object);
        var command = new DeleteUserCommand("USER001");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.IsActive.Should().BeFalse(); // User should be deactivated, not deleted
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var users = new List<User>();
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new DeleteUserCommandHandler(_mockContext.Object);
        var command = new DeleteUserCommand("NOTFOUND");

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldOnlyDeactivateSpecifiedUser_WhenMultipleExist()
    {
        // Arrange
        var user1 = new User { UserId = "USER001", FirstName = "John", LastName = "Doe", UserType = UserRole.USER, IsActive = true };
        var user2 = new User { UserId = "USER002", FirstName = "Jane", LastName = "Smith", UserType = UserRole.ADMIN, IsActive = true };
        var user3 = new User { UserId = "USER003", FirstName = "Bob", LastName = "Wilson", UserType = UserRole.USER, IsActive = true };

        var users = new List<User> { user1, user2, user3 };
        var mockDbSet = users.BuildMockDbSet();
        
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteUserCommandHandler(_mockContext.Object);
        var command = new DeleteUserCommand("USER002");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user1.IsActive.Should().BeTrue(); // Not affected
        user2.IsActive.Should().BeFalse(); // Deactivated
        user3.IsActive.Should().BeTrue(); // Not affected
    }
}
