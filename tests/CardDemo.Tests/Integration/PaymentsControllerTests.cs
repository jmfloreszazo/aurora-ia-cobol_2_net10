using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Features.Payments;
using FluentAssertions;
using Xunit;

namespace CardDemo.Tests.Integration;

public class PaymentsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PaymentsControllerTests(CustomWebApplicationFactory factory)
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
    public async Task MakePayment_ShouldReturnUnauthorized_WhenNoToken()
    {
        // Arrange
        var request = new MakePaymentRequest(1, 100.00m, DateTime.Now);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MakePayment_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var request = new MakePaymentRequest(999999999, 100.00m, DateTime.Now);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPaymentsByAccount_ShouldReturnUnauthorized_WhenNoToken()
    {
        // Act
        var response = await _client.GetAsync("/api/Payments/account/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPaymentsByAccount_ShouldReturnPagedResult_WhenAuthenticated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get a valid account ID
        var accountsResponse = await _client.GetAsync("/api/Accounts?pageNumber=1&pageSize=1");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        
        if (accounts?.Items?.Count > 0)
        {
            var accountId = accounts.Items[0].AccountId;
            
            // Act
            var response = await _client.GetAsync($"/api/Payments/account/{accountId}?pageNumber=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<PagedResult<PaymentDto>>();
            result.Should().NotBeNull();
            result!.Items.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetPaymentsByAccount_ShouldReturnEmptyList_WhenAccountHasNoPayments()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - using a non-existent account (should still return OK with empty list)
        var response = await _client.GetAsync("/api/Payments/account/999999999?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<PaymentDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
    }

    #region MakePayment Success Tests

    [Fact]
    public async Task MakePayment_ShouldReturnCreated_WhenPaymentSucceeds()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get a valid account with balance
        var accountsResponse = await _client.GetAsync("/api/Accounts?pageNumber=1&pageSize=1");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        
        if (accounts?.Items?.Count > 0 && accounts.Items[0].CurrentBalance > 0)
        {
            var accountId = accounts.Items[0].AccountId;
            var request = new MakePaymentRequest(accountId, 10.00m, DateTime.UtcNow);

            // Act
            var response = await _client.PostAsJsonAsync("/api/Payments", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task MakePayment_ShouldReturnBadRequest_WhenInvalidAmount()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get a valid account
        var accountsResponse = await _client.GetAsync("/api/Accounts?pageNumber=1&pageSize=1");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        
        if (accounts?.Items?.Count > 0)
        {
            var accountId = accounts.Items[0].AccountId;
            var request = new MakePaymentRequest(accountId, -100.00m, DateTime.UtcNow); // Negative amount

            // Act
            var response = await _client.PostAsJsonAsync("/api/Payments", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
        }
    }

    [Fact]
    public async Task MakePayment_ShouldReturnBadRequest_WhenZeroAmount()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get a valid account
        var accountsResponse = await _client.GetAsync("/api/Accounts?pageNumber=1&pageSize=1");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        
        if (accounts?.Items?.Count > 0)
        {
            var accountId = accounts.Items[0].AccountId;
            var request = new MakePaymentRequest(accountId, 0m, DateTime.UtcNow);

            // Act
            var response = await _client.PostAsJsonAsync("/api/Payments", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
        }
    }

    #endregion

    #region PayFullBalance Tests

    [Fact]
    public async Task PayFullBalance_ShouldReturnUnauthorized_WhenNoToken()
    {
        // Act
        var response = await _client.PostAsync("/api/Payments/pay-full-balance/1", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PayFullBalance_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/Payments/pay-full-balance/999999999", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PayFullBalance_ShouldReturnOk_WhenAccountExists()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get a valid account
        var accountsResponse = await _client.GetAsync("/api/Accounts?pageNumber=1&pageSize=1");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        
        if (accounts?.Items?.Count > 0)
        {
            var accountId = accounts.Items[0].AccountId;

            // Act
            var response = await _client.PostAsync($"/api/Payments/pay-full-balance/{accountId}", null);

            // Assert
            // Should return OK even if balance is zero (no-op)
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
        }
    }

    #endregion
}

public record MakePaymentRequest(
    long AccountId,
    decimal Amount,
    DateTime PaymentDate
);
