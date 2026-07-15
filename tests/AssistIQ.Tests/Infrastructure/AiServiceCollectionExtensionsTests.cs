using AssistIQ.Application.Abstractions;
using AssistIQ.Infrastructure.Ai;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AssistIQ.Tests.Infrastructure;

public sealed class AiServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAssistIQAi_WithoutProvider_ShouldRegisterFakeProvider()
    {
        using var services = BuildServices([]);

        services.GetRequiredService<IAiDraftService>().Should().BeOfType<FakeAiDraftService>();
    }

    [Fact]
    public void AddAssistIQAi_WithGitHubModelsProvider_ShouldRegisterGitHubProvider()
    {
        using var services = BuildServices(new Dictionary<string, string?>
        {
            ["AI_PROVIDER"] = "GitHubModels",
            ["GITHUB_MODELS_TOKEN"] = "github-test-token"
        });

        services.GetRequiredService<IAiDraftService>().Should().BeOfType<GitHubModelsAiDraftService>();
    }

    [Fact]
    public void AddAssistIQAi_ShouldIgnoreTokenFromConfigurationFiles()
    {
        using var services = BuildServices(new Dictionary<string, string?>
        {
            ["AI_PROVIDER"] = "GitHubModels",
            ["Ai:GitHubModels:Token"] = "must-not-be-bound"
        });

        services.GetRequiredService<Microsoft.Extensions.Options.IOptions<GitHubModelsOptions>>()
            .Value.Token.Should().NotBe("must-not-be-bound");
    }

    [Fact]
    public void AddAssistIQAi_WithUnknownProvider_ShouldRejectConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AI_PROVIDER"] = "Unknown"
            })
            .Build();
        var services = new ServiceCollection();

        var act = () => services.AddAssistIQAi(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unknown*");
    }

    private static ServiceProvider BuildServices(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAssistIQAi(configuration);
        return services.BuildServiceProvider();
    }
}
