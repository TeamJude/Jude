using Jude.Server.Data.Models;

namespace Jude.Server.Domains.Claims;

public record GetClaimsRequest(
    int Page = 1,
    int PageSize = 10,
    ClaimStatus[]? Status = null,
    FraudRiskLevel[]? RiskLevel = null,
    bool? RequiresHumanReview = null,
    string? Search = null
);

public record GetClaimsResponse(ClaimSummaryResponse[] Claims, int TotalCount);

public record ClaimSummaryResponse(
    Guid Id,
    string TransactionNumber,
    string PatientName,
    string MembershipNumber,
    string ProviderPractice,
    decimal ClaimAmount,
    decimal? ApprovedAmount,
    string Currency,
    ClaimStatus Status,
    ClaimSource Source,
    DateTime SubmittedAt,
    FraudRiskLevel FraudRiskLevel,
    bool IsFlagged,
    bool RequiresHumanReview,
    string? AgentRecommendation,
    DateTime IngestedAt,
    DateTime UpdatedAt
);

public record ClaimDetailResponse(
    Guid Id,
    string TransactionNumber,
    string ClaimNumber,
    string PatientName,
    string MembershipNumber,
    string ProviderPractice,
    decimal ClaimAmount,
    decimal? ApprovedAmount,
    string Currency,
    ClaimStatus Status,
    ClaimSource Source,
    DateTime? SubmittedAt,
    DateTime? ProcessedAt,
    string? AgentRecommendation,
    string? AgentReasoning,
    decimal? AgentConfidenceScore,
    DateTime? AgentProcessedAt,
    List<string>? FraudIndicators,
    FraudRiskLevel FraudRiskLevel,
    bool IsFlagged,
    bool RequiresHumanReview,
    ClaimDecision? FinalDecision,
    string? ReviewerComments,
    string? RejectionReason,
    DateTime? ReviewedAt,
    ReviewerInfo? ReviewedBy,
    DateTime IngestedAt,
    DateTime UpdatedAt,
    string? CIMASPayload
);

public record ReviewerInfo(
    Guid Id,
    string? Username,
    string Email
);


public enum ClaimsDashboardPeriod
{
    Last24Hours,
    Last7Days,
    Last30Days,
    LastQuarter
}

public record ClaimsDashboardRequest(
ClaimsDashboardPeriod Period
);

public record ClaimsDashboardResponse(
    int TotalClaims,
    double AutoApprovedRate,
    double AvgProcessingTime,
    int ClaimsFlagged,
    double TotalClaimsChangePercent,
    double AutoApprovedRateChangePercent,
    double AvgProcessingTimeChangePercent,
    double ClaimsFlaggedChangePercent,
    List<ClaimsActivityResponse> ClaimsActivity
);

public record ClaimsActivityResponse(
    string Day,
    int NewClaims,
    int Processed,
    int Approved,
    int Rejected
);

