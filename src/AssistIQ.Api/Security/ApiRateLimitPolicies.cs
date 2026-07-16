using System.Security.Claims;
using System.Threading.RateLimiting;
using AssistIQ.Application.Common;

namespace AssistIQ.Api.Security;

public static class ApiRateLimitPolicies
{
    public const string Login = "login";
    public const string AiDraft = "ai-draft";
    public const int LoginPermitLimit = 5;
    public const int AiDraftPermitLimit = 10;

    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public static RateLimitPartition<string> CreateLoginPartition(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return CreateFixedWindowPartition($"login:{remoteIp}", LoginPermitLimit);
    }

    public static RateLimitPartition<string> CreateAiDraftPartition(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        return CreateFixedWindowPartition($"ai-draft:{userId}", AiDraftPermitLimit);
    }

    public static async ValueTask WriteRejectedResponseAsync(
        HttpContext context,
        TimeSpan? retryAfter,
        CancellationToken cancellationToken)
    {
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (retryAfter is not null)
        {
            context.Response.Headers.RetryAfter = Math.Max(1, (int)Math.Ceiling(retryAfter.Value.TotalSeconds)).ToString();
        }

        await context.Response.WriteAsJsonAsync(new
        {
            errorCode = ErrorCodes.RateLimitExceeded,
            message = "Too many requests. Try again later.",
            correlationId = context.TraceIdentifier
        }, cancellationToken);
    }

    private static RateLimitPartition<string> CreateFixedWindowPartition(string key, int permitLimit)
    {
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = Window,
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        });
    }
}
