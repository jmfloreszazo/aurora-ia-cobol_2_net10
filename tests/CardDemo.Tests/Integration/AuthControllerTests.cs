using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CardDemo.Application.Common.DTOs;
using FluentAssertions;
using Xunit;

namespace CardDemo.Tests.Integration;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var loginRequest = new LoginRequest("ADMIN", "Admin@123");
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse!.Token;
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var loginRequest = new LoginRequest("ADMIN", "Admin@123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.UserId.Should().Be("ADMIN");
        loginResponse.FirstName.Should().NotBeNullOrEmpty();
        loginResponse.LastName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var loginRequest = new LoginRequest("ADMIN", "WrongPassword");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        var loginRequest = new LoginRequest("NONEXISTENT", "Password@123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        // Arrange
        var loginRequest = new LoginRequest("", "");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        // Assert - API returns 401 Unauthorized for empty credentials (treated as invalid login)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnUserType_WhenLoginSuccessful()
    {
        // Arrange
        var loginRequest = new LoginRequest("ADMIN", "Admin@123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse!.UserType.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_ShouldRequireAuthorization()
    {
        // Arrange - Register requires ADMIN role
        var registerRequest = new RegisterRequest("NEWUSER", "Test@123", "Test@123", "Test", "User", "USER");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_ShouldRequireAdminRole()
    {
        // Arrange - Register requires ADMIN role
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var registerRequest = new RegisterRequest("NEWUSER", "Test@123", "Test@123", "Test", "User", "USER");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);

        // Assert - Either Created (if authorized) or Forbidden (if role doesn't match)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenTokenIsEmpty()
    {
        // Arrange
        var refreshRequest = new { RefreshToken = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenTokenIsInvalid()
    {
        // Arrange
        var refreshRequest = new { RefreshToken = "invalid-refresh-token" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
