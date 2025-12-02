using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CardDemo.Application.Common.DTOs;
using FluentAssertions;
using Xunit;

namespace CardDemo.Tests.Integration;

public class AccountsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AccountsControllerTests(CustomWebApplicationFactory factory)
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
    public async Task GetAllAccounts_ShouldReturnUnauthorized_WhenNoToken()
    {
        // Act
        var response = await _client.GetAsync("/api/Accounts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllAccounts_ShouldReturnPagedResult_WhenAuthenticated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/Accounts?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetAccountById_ShouldReturnAccount_WhenExists()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // First get all accounts to find a valid ID
        var allResponse = await _client.GetAsync("/api/Accounts?pageNumber=1&pageSize=1");
        var allAccounts = await allResponse.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        
        if (allAccounts?.Items?.Count > 0)
        {
            var accountId = allAccounts.Items[0].AccountId;
            
            // Act
            var response = await _client.GetAsync($"/api/Accounts/{accountId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var account = await response.Content.ReadFromJsonAsync<AccountDetailResponse>();
            account.Should().NotBeNull();
            account!.AccountId.Should().Be(accountId);
        }
    }

    [Fact]
    public async Task GetAccountById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/Accounts/999999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAccountsByCustomerId_ShouldReturnAccounts_WhenCustomerExists()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/Accounts/customer/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var accounts = await response.Content.ReadFromJsonAsync<List<AccountDto>>();
        accounts.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAccountsByCustomerId_ShouldReturnEmptyList_WhenCustomerHasNoAccounts()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/Accounts/customer/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var accounts = await response.Content.ReadFromJsonAsync<List<AccountDto>>();
        accounts.Should().NotBeNull();
        accounts.Should().BeEmpty();
    }
}
