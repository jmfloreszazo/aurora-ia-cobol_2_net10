using System.Net.Http.Json;
using CardDemo.Application.Common.DTOs;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace CardDemo.Tests.SpecFlow.StepDefinitions;

[Binding]
public class CustomerSteps
{
    private readonly TestContext _context;

    public CustomerSteps(TestContext context)
    {
        _context = context;
    }

    [When(@"I request the list of all customers")]
    public async Task WhenIRequestTheListOfAllCustomers()
    {
        var response = await _context.Client.GetAsync("/api/Customers?pageNumber=1&pageSize=20");
        _context.LastHttpResponse = response;

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<PagedResult<CustomerResponse>>();
            _context.LastResponse = result;
        }
    }

    [When(@"I request customer with id ""([^""]*)""")]
    public async Task WhenIRequestCustomerWithId(int customerId)
    {
        var response = await _context.Client.GetAsync($"/api/Customers/{customerId}");
        _context.LastHttpResponse = response;

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<CustomerResponse>();
            _context.LastResponse = result;
        }
    }

    [Then(@"I should receive a list of customers")]
    public void ThenIShouldReceiveAListOfCustomers()
    {
        _context.LastResponse.Should().BeOfType<PagedResult<CustomerResponse>>();
        var result = (PagedResult<CustomerResponse>)_context.LastResponse!;
        result.Items.Should().NotBeNull();
    }

    [Then(@"I should receive customer details")]
    public void ThenIShouldReceiveCustomerDetails()
    {
        _context.LastResponse.Should().BeOfType<CustomerResponse>();
        var customer = (CustomerResponse)_context.LastResponse!;
        customer.CustomerId.Should().BeGreaterThan(0);
        customer.FullName.Should().NotBeNullOrEmpty();
    }

    [Then(@"the customer should have id ""([^""]*)""")]
    public void ThenTheCustomerShouldHaveId(int customerId)
    {
        _context.LastResponse.Should().BeOfType<CustomerResponse>();
        var customer = (CustomerResponse)_context.LastResponse!;
        customer.CustomerId.Should().Be(customerId);
    }

    [Then(@"I should receive a not found error")]
    public void ThenIShouldReceiveANotFoundError()
    {
        _context.LastHttpResponse.Should().NotBeNull();
        _context.LastHttpResponse!.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
