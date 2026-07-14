using System.Text.Json;
using AssistIQ.Application.Abstractions;
using AssistIQ.Domain.Audit;
using AssistIQ.Infrastructure.Persistence;

namespace AssistIQ.Infrastructure.Services;

public sealed class AuditService(AssistIQDbContext dbContext, ISystemClock clock) : IAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task RecordAsync(
        Guid? actorUserId,
        AuditAction action,
        string entityType,
        Guid entityId,
        object? before,
        object? after,
        CancellationToken cancellationToken)
    {
        var log = AuditLog.Create(
            actorUserId,
            action,
            entityType,
            entityId,
            clock.UtcNow,
            Serialize(before),
            Serialize(after));

        dbContext.AuditLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? Serialize(object? value)
    {
        return value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
    }
}
