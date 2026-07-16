namespace AssistIQ.Api.Security;

public sealed class AssistIQRateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int LoginPermitLimit { get; set; } = ApiRateLimitPolicies.LoginPermitLimit;

    public int AiDraftPermitLimit { get; set; } = ApiRateLimitPolicies.AiDraftPermitLimit;
}
