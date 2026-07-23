using AssistIQ.Domain.Users;
using Microsoft.AspNetCore.Authorization;

namespace AssistIQ.Api.Auth;

public static class AuthorizationPolicies
{
    public const string KnowledgeManage = nameof(KnowledgeManage);
    public const string TicketsManage = nameof(TicketsManage);
    public const string DraftsManage = nameof(DraftsManage);
    public const string AuditLogsView = nameof(AuditLogsView);
    public const string UsageLogsView = nameof(UsageLogsView);
    public const string AdminStatsView = nameof(AdminStatsView);

    public static void AddAssistIQPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(KnowledgeManage, policy => policy.RequireRole(nameof(UserRole.Admin)));
        options.AddPolicy(TicketsManage, policy => policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.SupportAgent)));
        options.AddPolicy(DraftsManage, policy => policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.SupportAgent)));
        options.AddPolicy(AuditLogsView, policy => policy.RequireRole(nameof(UserRole.Admin)));
        options.AddPolicy(UsageLogsView, policy => policy.RequireRole(nameof(UserRole.Admin)));
        options.AddPolicy(AdminStatsView, policy => policy.RequireRole(nameof(UserRole.Admin)));
    }
}
