using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace AssistIQ.Api.Security;

public static class RateLimitingServiceCollectionExtensions
{
    public static IServiceCollection AddAssistIQRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<AssistIQRateLimitingOptions>()
            .Bind(configuration.GetSection(AssistIQRateLimitingOptions.SectionName))
            .Validate(
                policyOptions => policyOptions.LoginPermitLimit > 0 && policyOptions.AiDraftPermitLimit > 0,
                "Rate limiting permit limits must be positive.")
            .ValidateOnStart();

        services.AddRateLimiter();
        services.AddOptions<RateLimiterOptions>()
            .Configure<IOptions<AssistIQRateLimitingOptions>>((options, configuredPolicyOptions) =>
        {
            var policyOptions = configuredPolicyOptions.Value;
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(
                ApiRateLimitPolicies.Login,
                context => ApiRateLimitPolicies.CreateLoginPartition(
                    context,
                    policyOptions.LoginPermitLimit));
            options.AddPolicy(
                ApiRateLimitPolicies.AiDraft,
                context => ApiRateLimitPolicies.CreateAiDraftPartition(
                    context,
                    policyOptions.AiDraftPermitLimit));
            options.OnRejected = async (context, cancellationToken) =>
            {
                TimeSpan? retryAfter = null;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue))
                {
                    retryAfter = retryAfterValue;
                }

                await ApiRateLimitPolicies.WriteRejectedResponseAsync(
                    context.HttpContext,
                    retryAfter,
                    cancellationToken);
            };
        });

        return services;
    }
}
