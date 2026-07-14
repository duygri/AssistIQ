using AssistIQ.Domain.Audit;

namespace AssistIQ.Application.AuditLogs;

public sealed record AuditLogDto(
    Guid Id,
    Guid? ActorUserId,
    AuditAction Action,
    string EntityName,
    Guid EntityId,
    DateTimeOffset OccurredAt,
    string? BeforeJson,
    string? AfterJson,
    string? MetadataJson);
