using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Jude.Server.Data.Models;

public class ClaimModel
{
    public Guid Id { get; set; }
    public DateTime IngestedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;
    public ClaimSource Source { get; set; } = ClaimSource.CIMAS;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public string? AgentRecommendation { get; set; }
    public string? AgentReasoning { get; set; }
    public decimal? AgentConfidenceScore { get; set; }
    public DateTime? AgentProcessedAt { get; set; }


    public bool IsFlagged { get; set; } = false;
    public List<string>? FraudIndicators { get; set; }
    public FraudRiskLevel FraudRiskLevel { get; set; } = FraudRiskLevel.Low;

    public bool RequiresHumanReview { get; set; } = true;
    public ClaimDecision? FinalDecision { get; set; }
    public string? ReviewerComments { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // [ForeignKey(nameof(ClaimDataModel))]
    // public Guid ClaimDataId { get; set; }
    // public ClaimDataModel ClaimData { get; set; }

    [ForeignKey(nameof(UserModel))]
    public Guid? ReviewedById { get; set; }
    public UserModel User { get; set; }
}

public enum ClaimStatus
{
    Pending,
    Processing,
    PendingReview,
    Approved,
    Rejected,
    RequestMoreInfo,
    Resubmitted,
    Completed,
}

public enum ClaimSource
{
    CIMAS,
    Manual,
}

public enum ClaimDecision
{
    Approve,
    Reject,
    RequestMoreInfo,
    Escalate,
}

public enum FraudRiskLevel
{
    Low,
    Medium,
    High,
    Critical,
}
