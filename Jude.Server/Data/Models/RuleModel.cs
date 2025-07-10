namespace Jude.Server.Data.Models;

using System.ComponentModel.DataAnnotations.Schema;

public class RuleModel
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RuleStatus Status { get; set; } = RuleStatus.Active;

    [ForeignKey(nameof(UserModel))]
    public Guid CreatedById { get; set; }
    public UserModel? CreatedBy { get; set; }
}

public enum RuleStatus
{
    Active,
    Inactive,
    Archived,
}
