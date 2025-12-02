using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CardDemo.Application.Common.DTOs;
using FluentAssertions;
using Xunit;

namespace CardDemo.Tests.Integration;

public class CardsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CardsControllerTests(CustomWebApplicationFactory factory)
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
    public async Task GetAllCards_ShouldReturnUnauthorized_WhenNoToken()
    {
        // Act
        var response = await _client.GetAsync("/api/Cards");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllCards_ShouldReturnPagedResult_WhenAuthenticated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/Cards?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CardDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCardByNumber_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/Cards/0000000000000000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCardsByAccount_ShouldReturnCards_WhenAccountHasCards()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // First get an account with cards
        var accountsResponse = await _client.GetAsync("/api/Accounts?pageNumber=1&pageSize=1");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        
        if (accounts?.Items?.Count > 0)
        {
            var accountId = accounts.Items[0].AccountId;
            
            // Act
            var response = await _client.GetAsync($"/api/Cards/account/{accountId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var cards = await response.Content.ReadFromJsonAsync<List<CardDto>>();
            cards.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetCardsByAccount_ShouldReturnEmptyList_WhenAccountHasNoCards()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - using a non-existent account
        var response = await _client.GetAsync("/api/Cards/account/999999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var cards = await response.Content.ReadFromJsonAsync<List<CardDto>>();
        cards.Should().NotBeNull();
        cards.Should().BeEmpty();
    }

    #region GetCardByNumber Tests

    [Fact]
    public async Task GetCardByNumber_ShouldReturnCard_WhenExists()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // First get all cards to find a valid card number
        var allResponse = await _client.GetAsync("/api/Cards?pageNumber=1&pageSize=1");
        var allCards = await allResponse.Content.ReadFromJsonAsync<PagedResult<CardDto>>();
        
        if (allCards?.Items?.Count > 0)
        {
            var cardNumber = allCards.Items[0].CardNumber;
            
            // Act
            var response = await _client.GetAsync($"/api/Cards/{cardNumber}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var card = await response.Content.ReadFromJsonAsync<CardDto>();
            card.Should().NotBeNull();
            card!.CardNumber.Should().Be(cardNumber);
        }
    }

    #endregion

    #region UpdateCard Tests

    [Fact]
    public async Task UpdateCard_ShouldReturnUnauthorized_WhenNoToken()
    {
        // Arrange
        var request = new { ActiveStatus = "Y" };

        // Act
        var response = await _client.PutAsJsonAsync("/api/Cards/1234567890123456", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCard_ShouldReturnOk_WhenCardExists()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // First get a valid card number
        var allResponse = await _client.GetAsync("/api/Cards?pageNumber=1&pageSize=1");
        var allCards = await allResponse.Content.ReadFromJsonAsync<PagedResult<CardDto>>();
        
        if (allCards?.Items?.Count > 0)
        {
            var cardNumber = allCards.Items[0].CardNumber;
            var request = new { ActiveStatus = "Y" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/Cards/{cardNumber}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task UpdateCard_ShouldReturnNotFound_WhenCardDoesNotExist()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new { ActiveStatus = "Y" };

        // Act
        var response = await _client.PutAsJsonAsync("/api/Cards/0000000000000000", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCard_ShouldUpdateActiveStatus_WhenProvided()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // First get a card
        var allResponse = await _client.GetAsync("/api/Cards?pageNumber=1&pageSize=1");
        var allCards = await allResponse.Content.ReadFromJsonAsync<PagedResult<CardDto>>();
        
        if (allCards?.Items?.Count > 0)
        {
            var cardNumber = allCards.Items[0].CardNumber;
            var request = new { ActiveStatus = "Y" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/Cards/{cardNumber}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<CardDto>();
            result.Should().NotBeNull();
            result!.ActiveStatus.Should().Be("Y");
        }
    }

    #endregion
}
