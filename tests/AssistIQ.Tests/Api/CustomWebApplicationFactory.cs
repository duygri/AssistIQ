using AssistIQ.Infrastructure.Persistence;
using AssistIQ.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AssistIQ.Tests.Api;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly bool _useProductionRateLimits;
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("assistiq_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public CustomWebApplicationFactory()
        : this(useProductionRateLimits: false)
    {
    }

    internal CustomWebApplicationFactory(bool useProductionRateLimits)
    {
        _useProductionRateLimits = useProductionRateLimits;
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
                ["ApplyMigrationsOnStartup"] = "false",
                ["SeedDemoDataOnStartup"] = "false",
                ["RateLimiting:LoginPermitLimit"] = _useProductionRateLimits ? "5" : "1000",
                ["RateLimiting:AiDraftPermitLimit"] = _useProductionRateLimits ? "10" : "1000"
            });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AssistIQDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        await seeder.SeedAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
