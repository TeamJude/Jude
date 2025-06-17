using System.ComponentModel.DataAnnotations.Schema;

namespace Jude.Server.Data.Models;

public class FraudIndicatorModel
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IndicatorStatus Status { get; set; } = IndicatorStatus.Active;

    [ForeignKey(nameof(UserModel))]
    public Guid CreatedById { get; set; }
}

public enum IndicatorStatus
{
    Active,
    Inactive,
    Archived,
}
