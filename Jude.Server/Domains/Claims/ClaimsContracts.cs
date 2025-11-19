using Jude.Server.Data.Models;

namespace Jude.Server.Domains.Claims;

public record GetClaimsRequest(int Page = 1, int PageSize = 10, ClaimStatus[]? Status = null, string? Search = null);

public record GetClaimsResponse(GetClaimResponse[] Claims, int TotalCount);

public record GetClaimResponse(
    Guid Id,
    string ClaimNumber,
    string ClaimLineNo,
    string PatientFirstName,
    string PatientSurname,
    string MemberNumber,
    string ProviderName,
    string PracticeNumber,
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
    string ClaimNumber,
    string PatientFirstName,
    string PatientSurname,
    string MemberNumber,
    string Ino,
    string Dis,
    string MedicalSchemeName,
    string OptionName,
    string PayerName,
    string ProviderName,
    string PracticeNumber,
    string InvoiceReference,
    string AsAtNetworks,
    string ReferringPractice,
    DateTime ServiceDate,
    DateTime AssessmentDate,
    DateTime DateReceived,
    string ClaimCode,
    string CodeDescription,
    int Units,
    string ScriptCode,
    string Icd10Code,
    decimal TotalClaimAmount,
    decimal PaidFromRiskAmount,
    decimal PaidFromThreshold,
    decimal PaidFromSavings,
    decimal RecoveryAmount,
    decimal TotalAmountPaid,
    decimal Tariff,
    decimal CoPayAmount,
    string PayTo,
    string Rej,
    string Rev,
    string AuthNo,
    string Dl,
    string ClaimLineNo,
    string DuplicateClaim,
    string DuplicateClaimLine,
    string PaperOrEdi,
    string AssessorName,
    string ClaimTypeCode,
    DateTime PatientBirthDate,
    int PatientCurrentAge,
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

public record UploadExcelResponse(
    int TotalRows,
    int SuccessfullyQueued,
    int Duplicates,
    int Failed,
    List<string> Errors
);

public record ClaimUploadError(int RowNumber, string Error);

public record SubmitHumanReviewRequest(ClaimDecision Decision, string Comments);

public record SubmitHumanReviewResponse(Guid Id, DateTime ReviewedAt, ClaimDecision Decision, string Comments);

public record AuditLogResponse(
    Guid Id,
    DateTime Timestamp,
    string Action,
    AuditActorType ActorType,
    string? ActorName,
    string Description,
    Dictionary<string, object>? Metadata
);

public record GetClaimAuditLogsResponse(AuditLogResponse[] AuditLogs);
