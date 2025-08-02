using System;
using System.Text.Json.Serialization;
using Jude.Server.Core.Helpers;

namespace Jude.Server.Domains.Audit;

public record CreateAuditRequest(
    string EntityType,
    Guid EntityId,
    string Action,
    string ActorType,
    Guid? ActorId,
    string ActorName,
    string Description,
    string? Metadata = null
);

public record AuditResponse(
    Guid Id,
    DateTime Timestamp,
    string EntityType,
    Guid EntityId,
    string Action,
    string ActorType,
    Guid? ActorId,
    string ActorName,
    string Description,
    string? Metadata
);

public record GetAuditRequest(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? EntityType = null,
    string? Action = null,
    string? ActorType = null,
    int Page = 1,
    int PageSize = 20
);

public record GetAuditResponse(
    AuditResponse[] Entries,
    int TotalCount,
    int Page,
    int PageSize
);
