using System.ComponentModel.DataAnnotations.Schema;

namespace Jude.Server.Data.Models;

public class ClaimModel
{
    public Guid Id { get; set; }
    public DateTime IngestedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string TransactionNumber { get; set; } = string.Empty;
    public string ClaimNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string MembershipNumber { get; set; } = string.Empty;
    public string ProviderPractice { get; set; } = string.Empty;
    public decimal ClaimAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string Currency { get; set; } = "USD";

    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;
    public ClaimSource Source { get; set; } = ClaimSource.CIMAS;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public string? CIMASPayload { get; set; }

    public string? AgentRecommendation { get; set; }
    public string? AgentReasoning { get; set; }
    public decimal? AgentConfidenceScore { get; set; }
    public DateTime? AgentProcessedAt { get; set; }

    public List<string>? FraudIndicators { get; set; }
    public FraudRiskLevel FraudRiskLevel { get; set; } = FraudRiskLevel.Low;
    public bool IsFlagged { get; set; } = false;

    public bool RequiresHumanReview { get; set; } = true;
    public ClaimDecision? FinalDecision { get; set; }
    public string? ReviewerComments { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }

    [ForeignKey(nameof(UserModel))]
    public Guid? ReviewedById { get; set; }
    public UserModel User { get; set; }
}

public enum ClaimStatus
{
    Pending,
    Processing,
    Review,
    Completed,
}

public enum ClaimSource
{
    CIMAS,
}

public enum ClaimDecision
{
    Approved,
    Rejected,
}

public enum FraudRiskLevel
{
    Low,
    Medium,
    High,
    Critical,
}
