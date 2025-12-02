using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Users.Queries;
using CardDemo.Domain.Entities;
using CardDemo.Domain.Enums;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Users;

public class GetUserByIdQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;

    public GetUserByIdQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            UserId = "USER001",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserRole.ADMIN,
            IsActive = true,
            IsLocked = false
        };

        var users = new List<User> { user };
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new GetUserByIdQueryHandler(_mockContext.Object);
        var query = new GetUserByIdQuery("USER001");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be("USER001");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.UserType.Should().Be("ADMIN");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        var users = new List<User>();
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new GetUserByIdQueryHandler(_mockContext.Object);
        var query = new GetUserByIdQuery("NOTFOUND");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectUser_WhenMultipleExist()
    {
        // Arrange
        var users = new List<User>
        {
            new User { UserId = "USER001", FirstName = "John", LastName = "Doe", UserType = UserRole.USER, IsActive = true },
            new User { UserId = "USER002", FirstName = "Jane", LastName = "Smith", UserType = UserRole.ADMIN, IsActive = true },
            new User { UserId = "USER003", FirstName = "Bob", LastName = "Wilson", UserType = UserRole.USER, IsActive = false }
        };

        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new GetUserByIdQueryHandler(_mockContext.Object);
        var query = new GetUserByIdQuery("USER002");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be("USER002");
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        result.UserType.Should().Be("ADMIN");
    }
}
