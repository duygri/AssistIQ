using AssistIQ.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AssistIQ.Infrastructure.Ai;

public static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddAssistIQAi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["AI_PROVIDER"]
            ?? configuration["Ai:Provider"]
            ?? "Fake";

        if (provider.Equals("Fake", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IAiDraftService, FakeAiDraftService>();
            return services;
        }

        if (provider.Equals("GitHubModels", StringComparison.OrdinalIgnoreCase))
        {
            services.Configure<GitHubModelsOptions>(options =>
            {
                var model = configuration["Ai:GitHubModels:Model"];
                if (!string.IsNullOrWhiteSpace(model))
                {
                    options.Model = model;
                }

                var token = Environment.GetEnvironmentVariable("GITHUB_MODELS_TOKEN");
                if (token is not null)
                {
                    options.Token = token;
                }
            });
            services.AddHttpClient<IAiDraftService, GitHubModelsAiDraftService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            return services;
        }

        throw new InvalidOperationException(
            $"Unsupported AI provider '{provider}'. Supported values are Fake and GitHubModels.");
    }
}
