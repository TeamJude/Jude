using System.ComponentModel.DataAnnotations.Schema;

namespace Jude.Server.Data.Models;

public class PolicyModel
{
    public int Id { get; set; }
    public string? DocumentId { get; set; }
    public required string Name { get; set; }
    public string? DocumentUrl { get; set; }
    public PolicyStatus Status { get; set; } = PolicyStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserModel))]
    public Guid CreatedById { get; set; }
    public UserModel? CreatedBy { get; set; }
}

public enum PolicyStatus
{
    Pending,
    Active,
    Failed,
    Archived,
}
