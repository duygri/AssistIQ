using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace AssistIQ.Api.Security;

public static class RateLimitingServiceCollectionExtensions
{
    public static IServiceCollection AddAssistIQRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(
                ApiRateLimitPolicies.Login,
                ApiRateLimitPolicies.CreateLoginPartition);
            options.AddPolicy(
                ApiRateLimitPolicies.AiDraft,
                ApiRateLimitPolicies.CreateAiDraftPartition);
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
