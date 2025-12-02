using System.Security.Claims;
using System.Text.Encodings.Web;
using CardDemo.Application.Common.Interfaces;
using CardDemo.Infrastructure;
using CardDemo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CardDemo.Tests.Integration;

/// <summary>
/// Factory that uses TestAuthHandler to bypass JWT authentication.
/// This allows testing endpoints that require specific roles.
/// </summary>
public class TestAuthWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"CardDemoTestAuthDb_{Guid.NewGuid()}";
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(DbContextOptions<CardDemoDbContext>) ||
                d.ServiceType == typeof(CardDemoDbContext) ||
                d.ServiceType == typeof(ICardDemoDbContext)).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }
        });

        builder.ConfigureTestServices(services =>
        {
            var sp = services.BuildServiceProvider();
            var configuration = sp.GetRequiredService<IConfiguration>();

            // Add Infrastructure services but skip DbContext
            services.AddInfrastructure(configuration, skipDbContext: true);

            // Register DbContext with InMemory database
            services.AddDbContext<CardDemoDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            services.AddScoped<ICardDemoDbContext>(provider => 
                provider.GetRequiredService<CardDemoDbContext>());
            
            // Replace JWT authentication with test authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, AdminTestAuthHandler>("TestScheme", options => { });

            // Build and seed the database
            var sp2 = services.BuildServiceProvider();

            using var scope = sp2.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<CardDemoDbContext>();

            db.Database.EnsureCreated();
            DatabaseSeeder.SeedAsync(db).Wait();
        });
    }
}

/// <summary>
/// Authentication handler for tests that simulates an Admin user with correct role case.
/// </summary>
public class AdminTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public AdminTestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) 
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check for custom header to skip auth (for testing unauthorized scenarios)
        if (Request.Headers.ContainsKey("X-Skip-Auth"))
        {
            return Task.FromResult(AuthenticateResult.Fail("No authentication"));
        }

        // Create claims for an Admin user with correct role case
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "ADMIN"),
            new Claim(ClaimTypes.Name, "Admin User"),
            new Claim(ClaimTypes.Role, "ADMIN"),  // Matches controller's [Authorize(Roles = "ADMIN")]
            new Claim("userId", "ADMIN"),
            new Claim("role", "ADMIN")
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
