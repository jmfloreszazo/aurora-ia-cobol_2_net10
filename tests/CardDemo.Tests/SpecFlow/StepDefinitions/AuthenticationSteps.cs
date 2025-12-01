using System.Net.Http.Headers;
using System.Net.Http.Json;
using CardDemo.Application.Common.DTOs;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace CardDemo.Tests.SpecFlow.StepDefinitions;

[Binding]
public class AuthenticationSteps
{
    private readonly TestContext _context;

    public AuthenticationSteps(TestContext context)
    {
        _context = context;
    }

    [Given(@"I am an anonymous user")]
    public void GivenIAmAnAnonymousUser()
    {
        _context.AuthToken = null;
        _context.Client.DefaultRequestHeaders.Authorization = null;
    }

    [Given(@"I am authenticated as ""([^""]*)""")]
    public async Task GivenIAmAuthenticatedAs(string userId)
    {
        var password = userId == "ADMIN" ? "Admin@123" : "User@123";
        await WhenILoginWithCredentials(userId, password);
        ThenTheLoginShouldBeSuccessful();
    }

    [When(@"I login with user id ""([^""]*)"" and password ""([^""]*)""")]
    public async Task WhenILoginWithCredentials(string userId, string password)
    {
        var loginRequest = new LoginRequest(userId, password);
        var response = await _context.Client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        _context.LastHttpResponse = response;

        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            _context.AuthToken = loginResponse!.Token;
            _context.LastResponse = loginResponse;
            _context.Client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _context.AuthToken);
        }
    }

    [Then(@"the login should be successful")]
    public void ThenTheLoginShouldBeSuccessful()
    {
        _context.LastHttpResponse.Should().NotBeNull();
        _context.LastHttpResponse!.IsSuccessStatusCode.Should().BeTrue();
        _context.AuthToken.Should().NotBeNullOrEmpty();
    }

    [Then(@"the login should fail")]
    public void ThenTheLoginShouldFail()
    {
        _context.LastHttpResponse.Should().NotBeNull();
        _context.LastHttpResponse!.IsSuccessStatusCode.Should().BeFalse();
        _context.AuthToken.Should().BeNullOrEmpty();
    }

    [Then(@"I should receive an authentication token")]
    public void ThenIShouldReceiveAnAuthenticationToken()
    {
        _context.LastResponse.Should().BeOfType<LoginResponse>();
        var response = (LoginResponse)_context.LastResponse!;
        response.Token.Should().NotBeNullOrEmpty();
    }

    [Then(@"the token should contain my user id ""([^""]*)""")]
    public void ThenTheTokenShouldContainMyUserId(string userId)
    {
        _context.LastResponse.Should().BeOfType<LoginResponse>();
        var response = (LoginResponse)_context.LastResponse!;
        response.UserId.Should().Be(userId);
    }

    [Then(@"the token should contain my full name")]
    public void ThenTheTokenShouldContainMyFullName()
    {
        _context.LastResponse.Should().BeOfType<LoginResponse>();
        var response = (LoginResponse)_context.LastResponse!;
        response.FirstName.Should().NotBeNullOrEmpty();
        response.LastName.Should().NotBeNullOrEmpty();
    }

    [Then(@"the token should contain my user type ""([^""]*)""")]
    public void ThenTheTokenShouldContainMyUserType(string userType)
    {
        _context.LastResponse.Should().BeOfType<LoginResponse>();
        var response = (LoginResponse)_context.LastResponse!;
        response.UserType.Should().Be(userType);
    }

    [Then(@"I should receive an unauthorized error")]
    public void ThenIShouldReceiveAnUnauthorizedError()
    {
        _context.LastHttpResponse.Should().NotBeNull();
        _context.LastHttpResponse!.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Then(@"I should receive a bad request error")]
    public void ThenIShouldReceiveABadRequestError()
    {
        _context.LastHttpResponse.Should().NotBeNull();
        _context.LastHttpResponse!.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
