using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jude.Server.Data.Models;

public class AgentReviewModel
{
    public Guid Id { get; set; }
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
    public DecisionStatus DecisionStatus { get; set; } = DecisionStatus.None;
    public string Recommendation { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; } = 0;
    
    [ForeignKey(nameof(ClaimModel))]
    public Guid ClaimId { get; set; }
    public ClaimModel Claim { get; set; } = null!;
}

public enum DecisionStatus
{
    None,
    Approved,
    Rejected,
} 