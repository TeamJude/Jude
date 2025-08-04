using System;
using System.ComponentModel.DataAnnotations;

namespace Jude.Server.Data.Models;

public enum AuditEntityType
{
    Claim,
    User,
    FraudIndicator,
    Policy,
    Rule,
}

public enum AuditActorType
{
    User,
    System,
    AiAgent
}

public class AuditLogModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public AuditEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public AuditActorType ActorType { get; set; }
    public Guid? ActorId { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}
