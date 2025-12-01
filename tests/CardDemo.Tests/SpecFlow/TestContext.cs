using CardDemo.Infrastructure.Persistence;
using CardDemo.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow;

namespace CardDemo.Tests.SpecFlow;

[Binding]
public class TestContext : IDisposable
{
    private CustomWebApplicationFactory? _factory;
    private HttpClient? _client;
    private IServiceScope? _scope;

    /// <summary>
    /// Resets the factory, client, and scope to ensure a fresh database for each scenario
    /// </summary>
    public void Reset()
    {
        _scope?.Dispose();
        _client?.Dispose();
        _factory?.Dispose();
        
        _scope = null;
        _client = null;
        _factory = null;
        AuthToken = null;
        RefreshToken = null;
        LastResponse = null;
        LastHttpResponse = null;
    }

    public CustomWebApplicationFactory Factory
    {
        get
        {
            _factory ??= new CustomWebApplicationFactory();
            return _factory;
        }
    }

    public HttpClient Client
    {
        get
        {
            _client ??= Factory.CreateClient();
            return _client;
        }
    }

    public CardDemoDbContext GetDbContext()
    {
        _scope ??= Factory.Services.CreateScope();
        return _scope.ServiceProvider.GetRequiredService<CardDemoDbContext>();
    }

    public string? AuthToken { get; set; }
    public string? RefreshToken { get; set; }
    public object? LastResponse { get; set; }
    public HttpResponseMessage? LastHttpResponse { get; set; }

    public void Dispose()
    {
        _scope?.Dispose();
        _client?.Dispose();
        _factory?.Dispose();
    }
}
