using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jude.Server.Data.Models;

public class HumanReviewModel
{
    public Guid Id { get; set; }
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
    public ClaimDecision Decision { get; set; } = ClaimDecision.None;
    public string Comments { get; set; } = string.Empty;

    [ForeignKey(nameof(ClaimModel))]
    public Guid ClaimId { get; set; }
    public ClaimModel Claim { get; set; } = null!;
}
