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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CardDemo.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Remover el registro de AddInfrastructure existente que incluye SQL Server
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
            // Get configuration for Infrastructure services
            var sp = services.BuildServiceProvider();
            var configuration = sp.GetRequiredService<IConfiguration>();

            // Add Infrastructure services (authentication, password hasher, etc) but skip DbContext
            services.AddInfrastructure(configuration, skipDbContext: true);

            // Registrar DbContext con InMemory database
            services.AddDbContext<CardDemoDbContext>(options =>
            {
                options.UseInMemoryDatabase("CardDemoTestDb");
            });

            services.AddScoped<ICardDemoDbContext>(provider => 
                provider.GetRequiredService<CardDemoDbContext>());
            
            // NO reemplazamos la autenticación JWT - dejamos que funcione normalmente
            // Esto permite que el endpoint /api/Auth/login funcione correctamente en tests
            
            // Build the service provider and seed the database
            var sp2 = services.BuildServiceProvider();

            using var scope = sp2.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<CardDemoDbContext>();

            // Ensure the database is created and seed it
            db.Database.EnsureCreated();
            DatabaseSeeder.SeedAsync(db).Wait();
        });
    }
}

/// <summary>
/// Authentication handler para tests que simula un usuario autenticado
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) 
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Crear claims para un usuario de test (ADMIN por defecto)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "ADMIN"),
            new Claim(ClaimTypes.Name, "Admin User"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("userId", "ADMIN"),
            new Claim("role", "Admin")
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
