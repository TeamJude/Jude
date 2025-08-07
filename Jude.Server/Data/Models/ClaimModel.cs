using System.ComponentModel.DataAnnotations.Schema;
using Jude.Server.Domains.Claims.Providers.CIMAS;

namespace Jude.Server.Data.Models;

public class ClaimModel
{
    public Guid Id { get; set; }
    public DateTime IngestedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ClaimResponse Data { get; set; } = new();
    public AgentReviewModel? AgentReview { get; set; }
    public HumanReviewModel? HumanReview { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

    public string TransactionNumber { get; set; } = string.Empty;
    public string ClaimNumber { get; set; } = string.Empty;
    public string PatientFirstName { get; set; } = string.Empty;
    public string PatientSurname { get; set; } = string.Empty;
    public string MedicalSchemeName { get; set; } = string.Empty;
    public decimal TotalClaimAmount { get; set; }

    [ForeignKey(nameof(UserModel))]
    public Guid? ReviewedById { get; set; }
    public UserModel? ReviewedBy { get; set; }
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
