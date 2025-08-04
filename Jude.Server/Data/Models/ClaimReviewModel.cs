using System.ComponentModel.DataAnnotations.Schema;

namespace Jude.Server.Data.Models;

public class ClaimReviewModel
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ClaimModel))]
    public Guid ClaimId { get; set; }
    public ClaimModel Claim { get; set; } = null!;

    [ForeignKey(nameof(UserModel))]
    public Guid ReviewerId { get; set; }
    public UserModel Reviewer { get; set; } = null!;

    public ClaimReviewDecision Decision { get; set; }
    public string Notes { get; set; } = string.Empty;

    // For tracking edits
    public DateTime? SubmittedAt { get; set; } = DateTime.UtcNow;
    public bool IsEdited { get; set; } = false;
}

public enum ClaimReviewDecision
{
    Approve,
    Reject,
    Pend,
    Partial
}
