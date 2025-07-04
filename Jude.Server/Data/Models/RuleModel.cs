namespace Jude.Server.Data.Models;

using System.ComponentModel.DataAnnotations.Schema;

public class RuleModel
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; } = 0;
    public RuleStatus Status { get; set; } = RuleStatus.Active;

    [ForeignKey(nameof(UserModel))]
    public Guid CreatedById { get; set; }
}

public enum RuleStatus
{
    Active,
    Inactive,
    Archived,
}
