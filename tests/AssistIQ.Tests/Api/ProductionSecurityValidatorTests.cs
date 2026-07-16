using AssistIQ.Api.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace AssistIQ.Tests.Api;

public sealed class ProductionSecurityValidatorTests
{
    [Theory]
    [InlineData("dev-only-signing-key-change-before-production-32chars", "Strong-Database-Password!", false)]
    [InlineData("too-short", "Strong-Database-Password!", false)]
    [InlineData("a-production-signing-key-with-at-least-32-bytes", "postgres", false)]
    [InlineData("a-production-signing-key-with-at-least-32-bytes", "Strong-Database-Password!", true)]
    public void Validate_WithUnsafeProductionConfiguration_ShouldFailClosed(
        string signingKey,
        string databasePassword,
        bool seedDemoData)
    {
        var configuration = BuildConfiguration(signingKey, databasePassword, seedDemoData);

        var act = () => ProductionSecurityValidator.Validate(configuration, isProduction: true);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Production security configuration is invalid*")
            .Which.Message.Should().NotContain(signingKey).And.NotContain(databasePassword);
    }

    [Fact]
    public void Validate_WithStrongProductionConfiguration_ShouldPass()
    {
        var configuration = BuildConfiguration(
            "a-production-signing-key-with-at-least-32-bytes",
            "Strong-Database-Password!",
            seedDemoData: false);

        var act = () => ProductionSecurityValidator.Validate(configuration, isProduction: true);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_InDevelopment_ShouldAllowDemoConfiguration()
    {
        var configuration = BuildConfiguration(
            "dev-only-signing-key-change-before-production-32chars",
            "postgres",
            seedDemoData: true);

        var act = () => ProductionSecurityValidator.Validate(configuration, isProduction: false);

        act.Should().NotThrow();
    }

    private static IConfiguration BuildConfiguration(
        string signingKey,
        string databasePassword,
        bool seedDemoData)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = signingKey,
                ["ConnectionStrings:DefaultConnection"] =
                    $"Host=localhost;Database=assistiq;Username=assistiq;Password={databasePassword}",
                ["SeedDemoDataOnStartup"] = seedDemoData.ToString()
            })
            .Build();
    }
}
