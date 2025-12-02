using System.Net;
using System.Net.Http.Json;
using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Features.Users;
using FluentAssertions;
using Xunit;

namespace CardDemo.Tests.Integration;

/// <summary>
/// Tests for UsersController with proper Admin role authentication.
/// Uses TestAuthWebApplicationFactory which bypasses JWT and uses correct role case.
/// </summary>
public class UsersControllerAdminTests : IClassFixture<TestAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersControllerAdminTests(TestAuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnOk_WhenAdminRoleIsCorrect()
    {
        // Act
        var response = await _client.GetAsync("/api/Users?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<UserDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnPaginatedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/Users?pageNumber=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<UserDto>>();
        result.Should().NotBeNull();
        result!.Items.Count().Should().BeGreaterThanOrEqualTo(1);
        result!.PageNumber.Should().Be(1);
        result!.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenUserExists()
    {
        // Act
        var response = await _client.GetAsync("/api/Users/ADMIN");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.UserId.Should().Be("ADMIN");
    }

    [Fact]
    public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/Users/NONEXISTENT_USER");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_ShouldCreateNewUser()
    {
        // Arrange
        var uniqueUserId = $"TEST{DateTime.UtcNow.Ticks % 10000}";
        var request = new
        {
            UserId = uniqueUserId,
            FirstName = "Test",
            LastName = "User",
            UserType = "USER",
            Password = "TestPass@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.UserId.Should().Be(uniqueUserId);
        user!.FirstName.Should().Be("Test");
        user!.LastName.Should().Be("User");
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenUserAlreadyExists()
    {
        // Arrange - ADMIN user already exists from seed
        var request = new
        {
            UserId = "ADMIN",
            FirstName = "Admin",
            LastName = "Duplicate",
            UserType = "ADMIN",
            Password = "Admin@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_ShouldUpdateExistingUser()
    {
        // Arrange - First create a user to update
        var uniqueUserId = $"UPDT{DateTime.UtcNow.Ticks % 10000}";
        var createRequest = new
        {
            UserId = uniqueUserId,
            FirstName = "Original",
            LastName = "Name",
            UserType = "USER",
            Password = "Pass@123"
        };
        await _client.PostAsJsonAsync("/api/Users", createRequest);

        var updateRequest = new
        {
            FirstName = "Updated",
            LastName = "LastName",
            UserType = "USER",
            Status = "ACTIVE",
            NewPassword = (string?)null
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Users/{uniqueUserId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.FirstName.Should().Be("Updated");
        user!.LastName.Should().Be("LastName");
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var updateRequest = new
        {
            FirstName = "Test",
            LastName = "User",
            UserType = "USER",
            Status = "ACTIVE",
            NewPassword = (string?)null
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/Users/NONEXISTENT_USER_XYZ", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_ShouldDeactivateUser()
    {
        // Arrange - First create a user to delete
        var uniqueUserId = $"DEL{DateTime.UtcNow.Ticks % 10000}";
        var createRequest = new
        {
            UserId = uniqueUserId,
            FirstName = "ToDelete",
            LastName = "User",
            UserType = "USER",
            Password = "Pass@123"
        };
        await _client.PostAsJsonAsync("/api/Users", createRequest);

        // Act
        var response = await _client.DeleteAsync($"/api/Users/{uniqueUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("/api/Users/NONEXISTENT_USER_DEL");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
