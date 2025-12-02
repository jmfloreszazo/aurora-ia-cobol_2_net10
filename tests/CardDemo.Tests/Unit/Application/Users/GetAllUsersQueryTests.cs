using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Users.Queries;
using CardDemo.Domain.Entities;
using CardDemo.Domain.Enums;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Users;

public class GetAllUsersQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;

    public GetAllUsersQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedUsers_WhenUsersExist()
    {
        // Arrange
        var users = new List<User>
        {
            new User { UserId = "USER001", FirstName = "John", LastName = "Doe", UserType = UserRole.ADMIN, IsActive = true },
            new User { UserId = "USER002", FirstName = "Jane", LastName = "Smith", UserType = UserRole.USER, IsActive = true },
            new User { UserId = "USER003", FirstName = "Bob", LastName = "Wilson", UserType = UserRole.USER, IsActive = false }
        };

        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new GetAllUsersQueryHandler(_mockContext.Object);
        var query = new GetAllUsersQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        // Arrange
        var users = new List<User>();
        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new GetAllUsersQueryHandler(_mockContext.Object);
        var query = new GetAllUsersQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldRespectPagination()
    {
        // Arrange
        var users = Enumerable.Range(1, 25).Select(i => new User
        {
            UserId = $"USER{i:D3}",
            FirstName = $"First{i}",
            LastName = $"Last{i}",
            UserType = i % 2 == 0 ? UserRole.ADMIN : UserRole.USER,
            IsActive = true
        }).ToList();

        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new GetAllUsersQueryHandler(_mockContext.Object);
        var query = new GetAllUsersQuery(2, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldMapUserProperties()
    {
        // Arrange
        var users = new List<User>
        {
            new User 
            { 
                UserId = "ADMIN01", 
                FirstName = "Admin", 
                LastName = "User", 
                UserType = UserRole.ADMIN,
                IsActive = true,
                IsLocked = false
            }
        };

        var mockDbSet = users.BuildMockDbSet();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        var handler = new GetAllUsersQueryHandler(_mockContext.Object);
        var query = new GetAllUsersQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        var user = result.Items.First();
        user.UserId.Should().Be("ADMIN01");
        user.FirstName.Should().Be("Admin");
        user.LastName.Should().Be("User");
        user.UserType.Should().Be("ADMIN");
        user.IsActive.Should().BeTrue();
    }
}
