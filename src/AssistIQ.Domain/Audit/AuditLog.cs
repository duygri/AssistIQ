namespace AssistIQ.Domain.Audit;

public sealed class AuditLog
{
    private AuditLog()
    {
        EntityName = string.Empty;
    }

    private AuditLog(
        Guid id,
        Guid? actorUserId,
        AuditAction action,
        string entityName,
        Guid entityId,
        DateTimeOffset occurredAt,
        string? beforeJson,
        string? afterJson,
        string? metadataJson)
    {
        Id = id;
        ActorUserId = actorUserId;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        OccurredAt = occurredAt;
        BeforeJson = beforeJson;
        AfterJson = afterJson;
        MetadataJson = metadataJson;
    }

    public Guid Id { get; private set; }

    public Guid? ActorUserId { get; private set; }

    public AuditAction Action { get; private set; }

    public string EntityName { get; private set; }

    public Guid EntityId { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public string? BeforeJson { get; private set; }

    public string? AfterJson { get; private set; }

    public string? MetadataJson { get; private set; }

    public static AuditLog Create(
        Guid? actorUserId,
        AuditAction action,
        string entityName,
        Guid entityId,
        DateTimeOffset occurredAt,
        string? beforeJson = null,
        string? afterJson = null,
        string? metadataJson = null)
    {
        if (string.IsNullOrWhiteSpace(entityName))
        {
            throw new InvalidOperationException("Audit entity name is required.");
        }

        return new AuditLog(
            Guid.NewGuid(),
            actorUserId,
            action,
            entityName.Trim(),
            entityId,
            occurredAt,
            beforeJson,
            afterJson,
            metadataJson);
    }
}
