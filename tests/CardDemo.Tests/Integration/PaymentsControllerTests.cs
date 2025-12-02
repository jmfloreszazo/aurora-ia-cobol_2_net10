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
}

public record MakePaymentRequest(
    long AccountId,
    decimal Amount,
    DateTime PaymentDate
);
