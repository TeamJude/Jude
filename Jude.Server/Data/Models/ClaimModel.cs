using System.ComponentModel.DataAnnotations.Schema;

namespace Jude.Server.Data.Models;

public class ClaimModel
{
    // Existing Properties
    public Guid Id { get; set; }
    public DateTime IngestedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public AgentReviewModel? AgentReview { get; set; }
    public HumanReviewModel? HumanReview { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

    public string ClaimNumber { get; set; } = string.Empty;
    public string PatientFirstName { get; set; } = string.Empty;
    public string PatientSurname { get; set; } = string.Empty;

    // Patient Identifier
    public string MemberNumber { get; set; } = string.Empty;

    public string MedicalSchemeName { get; set; } = string.Empty;
    public string OptionName { get; set; } = string.Empty;
    public string PayerName { get; set; } = string.Empty;

    // Amounts
    public decimal TotalClaimAmount { get; set; }
    public decimal TotalAmountPaid { get; set; }
    public decimal CoPayAmount { get; set; }

    // Provider/Invoice
    public string ProviderName { get; set; } = string.Empty;
    public string PracticeNumber { get; set; } = string.Empty;
    public string InvoiceReference { get; set; } = string.Empty;

    // Dates
    public DateTime ServiceDate { get; set; }
    public DateTime AssessmentDate { get; set; }
    public DateTime DateReceived { get; set; }

    // Service Details
    public string ClaimCode { get; set; } = string.Empty;
    public string CodeDescription { get; set; } = string.Empty;
    public int Units { get; set; }

    // Review/Source
    [ForeignKey(nameof(UserModel))]
    public Guid? ReviewedById { get; set; }
    public UserModel? ReviewedBy { get; set; }
    public string AssessorName { get; set; } = string.Empty;

    public ClaimSource Source { get; set; } = ClaimSource.CIMAS;
    public string ClaimTypeCode { get; set; } = string.Empty;
    public string? ClaimMarkdown { get; set; }

    public DateTime PatientBirthDate { get; set; }
    public int PatientCurrentAge { get; set; }
}

public enum ClaimStatus
{
    Pending,
    UnderAgentReview,
    UnderHumanReview,
    Approved,
    Rejected,
    Completed,
    Failed,
}

public enum ClaimSource
{
    CIMAS,
    Upload,
}
