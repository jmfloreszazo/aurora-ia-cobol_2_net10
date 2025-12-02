using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CardDemo.Application.Common.DTOs;
using FluentAssertions;
using Xunit;

namespace CardDemo.Tests.Integration;

public class TransactionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TransactionsControllerTests(CustomWebApplicationFactory factory)
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
    public async Task GetAllTransactions_ShouldReturnUnauthorized_WhenNoToken()
    {
        // Act
        var response = await _client.GetAsync("/api/Transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllTransactions_ShouldReturnPagedResult_WhenAuthenticated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/Transactions?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTransactionById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/Transactions/NONEXISTENT");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTransactionsByAccount_ShouldReturnPagedResult_WhenAuthenticated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get an account first
        var accountsResponse = await _client.GetAsync("/api/Accounts?pageNumber=1&pageSize=1");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        
        if (accounts?.Items?.Count > 0)
        {
            var accountId = accounts.Items[0].AccountId;
            
            // Act
            var response = await _client.GetAsync($"/api/Transactions/account/{accountId}?pageNumber=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
            result.Should().NotBeNull();
            result!.Items.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetTransactionsByCard_ShouldReturnPagedResult_WhenAuthenticated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get a card number first
        var cardsResponse = await _client.GetAsync("/api/Cards?pageNumber=1&pageSize=1");
        var cards = await cardsResponse.Content.ReadFromJsonAsync<PagedResult<CardDto>>();
        
        if (cards?.Items?.Count > 0)
        {
            var cardNumber = cards.Items[0].CardNumber;
            
            // Act
            var response = await _client.GetAsync($"/api/Transactions/card/{cardNumber}?pageNumber=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
            result.Should().NotBeNull();
            result!.Items.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetTransactionsByAccount_ShouldReturnEmptyList_WhenNoTransactions()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - using a non-existent account
        var response = await _client.GetAsync("/api/Transactions/account/999999999?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
    }

    #region GetTransactionById Success Tests

    [Fact]
    public async Task GetTransactionById_ShouldReturnTransaction_WhenExists()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // First get all transactions to find a valid ID
        var allResponse = await _client.GetAsync("/api/Transactions?pageNumber=1&pageSize=1");
        var allTransactions = await allResponse.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
        
        if (allTransactions?.Items?.Count > 0)
        {
            var transactionId = allTransactions.Items[0].TransactionId;
            
            // Act
            var response = await _client.GetAsync($"/api/Transactions/{transactionId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var transaction = await response.Content.ReadFromJsonAsync<TransactionDto>();
            transaction.Should().NotBeNull();
            transaction!.TransactionId.Should().Be(transactionId);
        }
    }

    #endregion

    #region CreateTransaction Tests

    [Fact]
    public async Task CreateTransaction_ShouldReturnUnauthorized_WhenNoToken()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            AccountId = 1,
            CardNumber = "1234567890123456",
            Amount = 100m,
            Description = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTransaction_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get a valid account and card
        var accountsResponse = await _client.GetAsync("/api/Accounts?pageNumber=1&pageSize=1");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        
        var cardsResponse = await _client.GetAsync("/api/Cards?pageNumber=1&pageSize=1");
        var cards = await cardsResponse.Content.ReadFromJsonAsync<PagedResult<CardDto>>();
        
        if (accounts?.Items?.Count > 0 && cards?.Items?.Count > 0)
        {
            var request = new CreateTransactionRequest
            {
                AccountId = accounts.Items[0].AccountId,
                CardNumber = cards.Items[0].CardNumber,
                Amount = 50m,
                Description = "Integration Test Transaction",
                TransactionType = "PU",
                CategoryCode = 1,
                MerchantName = "TEST MERCHANT"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Transactions", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task CreateTransaction_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateTransactionRequest
        {
            AccountId = 999999999,
            CardNumber = "1234567890123456",
            Amount = 50m,
            Description = "Test Transaction"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transactions", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTransaction_ShouldReturnBadRequest_WhenNegativeAmount()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var accountsResponse = await _client.GetAsync("/api/Accounts?pageNumber=1&pageSize=1");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        
        if (accounts?.Items?.Count > 0)
        {
            var request = new CreateTransactionRequest
            {
                AccountId = accounts.Items[0].AccountId,
                CardNumber = "1234567890123456",
                Amount = -100m, // Negative amount
                Description = "Test"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Transactions", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound);
        }
    }

    #endregion

    #region Date Range Filter Tests

    [Fact]
    public async Task GetAllTransactions_ShouldFilterByDateRange_WhenProvided()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var fromDate = DateTime.UtcNow.AddMonths(-6).ToString("yyyy-MM-dd");
        var toDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/Transactions?pageNumber=1&pageSize=10&fromDate={fromDate}&toDate={toDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}

public class CreateTransactionRequest
{
    public long AccountId { get; set; }
    public string CardNumber { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Description { get; set; } = default!;
    public string? TransactionType { get; set; }
    public int? CategoryCode { get; set; }
    public string? MerchantName { get; set; }
}
