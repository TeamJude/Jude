using Jude.Server.Data.Models;
using Jude.Server.Domains.Claims.Providers.CIMAS;

namespace Jude.Server.Domains.Claims;

// Input DTOs
public record CreateClaimFromCIMASRequest(
    ClaimRequest CIMASRequest,
    ClaimResponse? CIMASResponse = null
);

public record UpdateClaimStatusRequest(
    ClaimStatus Status,
    string? Comments = null
);

public record ReviewClaimRequest(
    ClaimDecision Decision,
    string? ReviewerComments = null,
    string? RejectionReason = null
);

public record ProcessClaimRequest(
    string AgentRecommendation,
    string AgentReasoning,
    decimal AgentConfidenceScore,
    string? AgentAnalysisPayload = null
);

public record GetClaimsRequest(
    int Page = 1,
    int PageSize = 10,
    ClaimStatus? Status = null,
    ClaimSource? Source = null,
    bool? RequiresReview = null,
    bool? IsFlagged = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? PatientName = null,
    string? ClaimNumber = null
);

// Response DTOs
public record GetClaimsResponse(
    ClaimSummaryDto[] Claims, 
    int TotalCount,
    ClaimStatsDto Stats
);

public record ClaimSummaryDto(
    Guid Id,
    string ClaimNumber,
    string PatientName,
    string ProviderName,
    decimal ClaimAmount,
    decimal? ApprovedAmount,
    ClaimStatus Status,
    FraudRiskLevel FraudRiskLevel,
    bool RequiresHumanReview,
    DateTime CreatedAt,
    DateTime? ReviewedAt,
    string? AgentRecommendation
);

public record ClaimDetailDto(
    Guid Id,
    string ClaimNumber,
    string TransactionNumber,
    
    // Patient Information
    int MembershipNumber,
    int DependantCode,
    string PatientName,
    string PatientIdNumber,
    
    // Provider Information
    string ProviderPracticeNumber,
    string ProviderPracticeName,
    
    // Financial Information
    decimal ClaimAmount,
    decimal ApprovedAmount,
    decimal PatientPayAmount,
    string Currency,
    
    // Status and Workflow
    ClaimStatus Status,
    ClaimSource Source,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? SubmittedAt,
    DateTime? ProcessedAt,
    
    // AI Agent Analysis
    string? AgentRecommendation,
    string? AgentReasoning,
    decimal? AgentConfidenceScore,
    DateTime? AgentProcessedAt,
    
    // Fraud Detection
    bool IsFlagged,
    FraudRiskLevel FraudRiskLevel,
    string[] FraudIndicators,
    
    // Human Review
    bool RequiresHumanReview,
    ClaimDecision? FinalDecision,
    string? ReviewerComments,
    string? RejectionReason,
    DateTime? ReviewedAt,
    string? ReviewedByName,
    
    // CIMAS Integration
    bool IsResubmitted,
    DateTime? ResubmittedAt,
    
    // Payloads (for detailed view)
    object? CIMASRequest,
    object? CIMASResponse,
    object? AgentAnalysis
);

public record ClaimStatsDto(
    int TotalClaims,
    int PendingClaims,
    int PendingReviewClaims,
    int ApprovedClaims,
    int RejectedClaims,
    int FlaggedClaims,
    decimal TotalClaimAmount,
    decimal TotalApprovedAmount,
    double AverageProcessingTime
);

// Batch operations
public record BatchReviewClaimsRequest(
    Guid[] ClaimIds,
    ClaimDecision Decision,
    string? Comments = null
);

public record ResubmitClaimRequest(
    Guid ClaimId,
    ClaimRequest ModifiedCIMASRequest
); 