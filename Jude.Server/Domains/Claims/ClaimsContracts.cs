using Jude.Server.Data.Models;
using Jude.Server.Domains.Claims.Providers.CIMAS;

namespace Jude.Server.Domains.Claims;

public record GetClaimsRequest(int Page = 1, int PageSize = 10, ClaimStatus[]? Status = null);

public record GetClaimsResponse(GetClaimResponse[] Claims, int TotalCount);

public record GetClaimResponse(
    Guid Id,
    string TransactionNumber,
    string ClaimNumber,
    string PatientFirstName,
    string PatientSurname,
    string MedicalSchemeName,
    decimal TotalClaimAmount,
    ClaimStatus Status,
    DateTime IngestedAt,
    DateTime UpdatedAt
);

public record GetClaimDetailResponse(
    Guid Id,
    DateTime IngestedAt,
    DateTime UpdatedAt,
    ClaimStatus Status,
    ClaimResponse Data,
    string TransactionNumber,
    string ClaimNumber,
    string PatientFirstName,
    string PatientSurname,
    string MedicalSchemeName,
    decimal TotalClaimAmount,
    AgentReviewResponse? AgentReview,
    HumanReviewResponse? HumanReview,
    ReviewerInfo? ReviewedBy,
    ClaimSource Source,
    string? ClaimMarkdown
);

public record AgentReviewResponse(
    Guid Id,
    DateTime ReviewedAt,
    ClaimDecision DecisionStatus,
    string Recommendation,
    string Reasoning,
    decimal ConfidenceScore
);

public record HumanReviewResponse(
    Guid Id,
    DateTime ReviewedAt,
    ClaimDecision DecisionStatus,
    string Comments
);

public record ReviewerInfo(Guid Id, string? Username, string Email);

public enum ClaimsDashboardPeriod
{
    Last24Hours,
    Last7Days,
    Last30Days,
    LastQuarter,
}

public record ClaimsDashboardRequest(ClaimsDashboardPeriod Period);

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

public record CreateOrUpdateClaimReviewRequest(
    Guid ClaimId,
    ClaimReviewDecision Decision,
    string Notes
);

public record ClaimReviewResponse(
    Guid Id,
    Guid ClaimId,
    ReviewerInfo Reviewer,
    ClaimReviewDecision Decision,
    string Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? SubmittedAt,
    bool IsEdited
);

public record GetClaimReviewsResponse(ClaimReviewResponse[] Reviews);
