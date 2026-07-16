using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace AssistIQ.Tests.Api;

public sealed class ProductionPipelineTests
{
    [Fact]
    public async Task Health_InProductionOverHttps_ShouldIncludeHstsHeader()
    {
        await using var factory = new ProductionWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://assistiq.example"),
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        response.Headers.Contains("Strict-Transport-Security").Should().BeTrue();
    }

    private sealed class ProductionWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.ConfigureAppConfiguration(configuration =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SigningKey"] = "a-production-signing-key-with-at-least-32-bytes",
                    ["ConnectionStrings:DefaultConnection"] =
                        "Host=localhost;Database=assistiq;Username=assistiq;Password=Strong-Database-Password!",
                    ["SeedDemoDataOnStartup"] = "false",
                    ["ApplyMigrationsOnStartup"] = "false"
                });
            });
        }
    }
}
