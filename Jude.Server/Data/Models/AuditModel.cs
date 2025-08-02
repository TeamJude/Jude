using System;

namespace Jude.Server.Data.Models;

public class AuditModel
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EntityType { get; set; } = string.Empty; // "Claim", "User", "FraudIndicator", etc.
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty; // "Created", "Updated", "Viewed", "StatusChanged", etc.
    public string ActorType { get; set; } = string.Empty; // "User", "System", "AI Agent"
    public Guid? ActorId { get; set; } // Null for System/AI Agent actions
    public string ActorName { get; set; } = string.Empty; // "John Smith", "AI Agent", "System"
    public string Description { get; set; } = string.Empty; // Human-readable description
    public string? Metadata { get; set; } // JSON for additional data
}
