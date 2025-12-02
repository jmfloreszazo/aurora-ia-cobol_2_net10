using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Features.BatchJobs;
using CardDemo.Api.Controllers;
using FluentAssertions;
using Xunit;

namespace CardDemo.Tests.Integration;

public class BatchJobsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BatchJobsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var loginRequest = new LoginRequest("ADMIN", "Admin@123");
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse!.Token;
    }

    private async Task<string> GetUserTokenAsync()
    {
        var loginRequest = new LoginRequest("USER01", "User@123");
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse!.Token;
    }

    #region Post Transactions Tests

    [Fact]
    public async Task PostTransactions_ShouldReturnUnauthorized_WhenNoToken()
    {
        // Act
        var response = await _client.PostAsync("/api/BatchJobs/post-transactions", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostTransactions_ShouldReturnForbidden_WhenNotAdmin()
    {
        // Arrange
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/BatchJobs/post-transactions", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostTransactions_ShouldReturnOk_WhenAdminRole()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/BatchJobs/post-transactions", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<BatchJobResult>();
        result.Should().NotBeNull();
        result!.JobName.Should().Be("TRANSACTION-POSTING");
        result.Status.Should().BeOneOf(BatchJobStatus.Completed, BatchJobStatus.CompletedWithErrors);
    }

    #endregion

    #region Calculate Interest Tests

    [Fact]
    public async Task CalculateInterest_ShouldReturnUnauthorized_WhenNoToken()
    {
        // Act
        var response = await _client.PostAsync("/api/BatchJobs/calculate-interest", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CalculateInterest_ShouldReturnForbidden_WhenNotAdmin()
    {
        // Arrange
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/BatchJobs/calculate-interest", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CalculateInterest_ShouldReturnOk_WhenAdminRole()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/BatchJobs/calculate-interest", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<BatchJobResult>();
        result.Should().NotBeNull();
        result!.JobName.Should().Be("INTEREST-CALCULATION");
    }

    [Fact]
    public async Task CalculateInterest_ShouldAcceptDateParameter()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var testDate = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");

        // Act
        var response = await _client.PostAsync($"/api/BatchJobs/calculate-interest?date={testDate}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Generate Statements Tests

    [Fact]
    public async Task GenerateStatements_ShouldReturnUnauthorized_WhenNoToken()
    {
        // Act
        var response = await _client.PostAsync("/api/BatchJobs/generate-statements", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateStatements_ShouldReturnForbidden_WhenNotAdmin()
    {
        // Arrange
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/BatchJobs/generate-statements", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GenerateStatements_ShouldReturnOk_WhenAdminRole()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/BatchJobs/generate-statements", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<BatchJobResult>();
        result.Should().NotBeNull();
        result!.JobName.Should().Be("STATEMENT-GENERATION");
    }

    [Fact]
    public async Task GenerateStatements_ShouldReturnBadRequest_WhenInvalidMonth()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/BatchJobs/generate-statements?month=13", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateStatements_ShouldAcceptYearAndMonthParameters()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/BatchJobs/generate-statements?year=2024&month=11", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Export Tests

    [Fact]
    public async Task ExportAccounts_ShouldReturnUnauthorized_WhenNoToken()
    {
        // Act
        var response = await _client.PostAsync("/api/BatchJobs/export/accounts", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExportAccounts_ShouldReturnOk_WhenAdminRole()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/BatchJobs/export/accounts", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<BatchJobResult>();
        result.Should().NotBeNull();
        result!.JobName.Should().StartWith("EXPORT-");
    }

    [Fact]
    public async Task ExportAccounts_ShouldAcceptFormatParameter()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - CSV format
        var responseCsv = await _client.PostAsync("/api/BatchJobs/export/accounts?format=CSV", null);

        // Assert
        responseCsv.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - JSON format
        var responseJson = await _client.PostAsync("/api/BatchJobs/export/accounts?format=Json", null);

        // Assert
        responseJson.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExportTransactions_ShouldReturnOk_WhenAdminRole()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/BatchJobs/export/transactions", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExportTransactions_ShouldAcceptDateRangeParameters()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var fromDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var toDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.PostAsync($"/api/BatchJobs/export/transactions?fromDate={fromDate}&toDate={toDate}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExportCustomers_ShouldReturnOk_WhenAdminRole()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/BatchJobs/export/customers", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Nightly Batch Tests

    [Fact]
    public async Task RunNightlyBatch_ShouldReturnUnauthorized_WhenNoToken()
    {
        // Act
        var response = await _client.PostAsync("/api/BatchJobs/run-nightly-batch", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RunNightlyBatch_ShouldReturnOk_WhenAdminRole()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/BatchJobs/run-nightly-batch", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<NightlyBatchResult>();
        result.Should().NotBeNull();
        result!.TransactionPosting.Should().NotBeNull();
        result.InterestCalculation.Should().NotBeNull();
    }

    #endregion

    #region Job History Tests

    [Fact]
    public async Task GetJobHistory_ShouldReturnHistory_WhenAdminRole()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // First run a job to ensure there's history
        await _client.PostAsync("/api/BatchJobs/calculate-interest", null);

        // Act
        var response = await _client.GetAsync("/api/BatchJobs/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var history = await response.Content.ReadFromJsonAsync<List<BatchJobResult>>();
        history.Should().NotBeNull();
    }

    [Fact]
    public async Task GetJobHistory_ShouldAcceptLimitParameter()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/BatchJobs/history?limit=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var history = await response.Content.ReadFromJsonAsync<List<BatchJobResult>>();
        history.Should().NotBeNull();
        history!.Count.Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    public async Task GetJob_ShouldReturnNotFound_WhenJobDoesNotExist()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/BatchJobs/non-existent-job-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
