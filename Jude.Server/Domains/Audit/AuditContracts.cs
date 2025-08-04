using Jude.Server.Data.Models;

namespace Jude.Server.Domains.Audit;

public record GetAuditLogsRequest(
    int Page = 1,
    int PageSize = 10,
    AuditEntityType? EntityType = null,
    Guid? EntityId = null,
    AuditActorType? ActorType = null,
    Guid? ActorId = null,
    string? Action = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
);

public record GetAuditLogsResponse(AuditLogResponse[] AuditLogs, int TotalCount);

public record AuditLogResponse(
    Guid Id,
    DateTime Timestamp,
    AuditEntityType EntityType,
    Guid EntityId,
    string Action,
    AuditActorType ActorType,
    Guid? ActorId,
    string Description,
    Dictionary<string, object>? Metadata
);

public record CreateAuditLogRequest(
    AuditEntityType EntityType,
    Guid EntityId,
    string Action,
    AuditActorType ActorType,
    Guid? ActorId,
    string Description,
    Dictionary<string, object>? Metadata = null
);
