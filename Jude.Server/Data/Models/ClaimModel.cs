using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Jude.Server.Data.Models;

public class ClaimModel
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // CIMAS Integration Fields
    public string ClaimNumber { get; set; } = string.Empty;
    public string TransactionNumber { get; set; } = string.Empty;
    public string CIMASRequestPayload { get; set; } = string.Empty; // JSON of ClaimRequest
    public string CIMASResponsePayload { get; set; } = string.Empty; // JSON of ClaimResponse
    
    // Patient/Member Information (extracted for easy querying)
    public int MembershipNumber { get; set; }
    public int DependantCode { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientIdNumber { get; set; } = string.Empty;
    public string ProviderPracticeNumber { get; set; } = string.Empty;
    public string ProviderPracticeName { get; set; } = string.Empty;

    // Financial Information
    public decimal ClaimAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal PatientPayAmount { get; set; }
    public string Currency { get; set; } = "USD";

    // Claim Status and Workflow
    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;
    public ClaimSource Source { get; set; } = ClaimSource.CIMAS;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    
    // AI Agent Analysis
    public string? AgentRecommendation { get; set; } // Approve/Reject/RequestInfo
    public string? AgentReasoning { get; set; } // AI's reasoning for the recommendation
    public decimal? AgentConfidenceScore { get; set; } // 0.0 to 1.0
    public string? AgentAnalysisPayload { get; set; } // JSON of detailed AI analysis
    public DateTime? AgentProcessedAt { get; set; }

    // Fraud Detection
    public bool IsFlagged { get; set; } = false;
    public string? FraudIndicators { get; set; } // JSON array of triggered fraud indicators
    public FraudRiskLevel FraudRiskLevel { get; set; } = FraudRiskLevel.Low;

    // Human Review and Approval
    public bool RequiresHumanReview { get; set; } = true;
    public ClaimDecision? FinalDecision { get; set; }
    public string? ReviewerComments { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Audit Trail
    public string? AuditLog { get; set; } // JSON array of status changes and actions
    
    // CIMAS Resubmission
    public bool IsResubmitted { get; set; } = false;
    public DateTime? ResubmittedAt { get; set; }
    public string? ResubmissionPayload { get; set; } // Modified claim data for resubmission

    // Foreign Keys
    [ForeignKey(nameof(UserModel))]
    public Guid? ProcessedById { get; set; } // User who processed the claim
    
    [ForeignKey(nameof(UserModel))]
    public Guid? ReviewedById { get; set; } // User who reviewed the claim
}

public enum ClaimStatus
{
    Pending,           // Initial state, waiting for processing
    Processing,        // Being analyzed by AI agent
    PendingReview,     // Waiting for human review
    Approved,          // Claim approved
    Rejected,          // Claim rejected
    RequestMoreInfo,   // More information needed
    Resubmitted,       // Claim has been resubmitted to CIMAS
    Completed,         // Final state - claim fully processed
    Cancelled          // Claim was cancelled
}

public enum ClaimSource
{
    CIMAS,             // Claim from CIMAS provider
    Manual,            // Manually entered claim
    API                // From other API sources
}

public enum ClaimDecision
{
    Approve,
    Reject,
    RequestMoreInfo,
    Escalate
}

public enum FraudRiskLevel
{
    Low,
    Medium,
    High,
    Critical
} 