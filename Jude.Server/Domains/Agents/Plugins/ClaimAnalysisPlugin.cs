using System.ComponentModel;
using Jude.Server.Data.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Jude.Server.Domains.Agents.Plugins;

public class ClaimAnalysisPlugin
{
    private readonly ClaimModel _claim;
    private readonly ILogger<ClaimAnalysisPlugin> _logger;

    public ClaimAnalysisPlugin(ClaimModel claim, ILogger<ClaimAnalysisPlugin> logger)
    {
        _claim = claim;
        _logger = logger;
    }

    [KernelFunction]
    [Description("Record detailed analysis reasoning for the claim")]
    public Task<string> UpdateClaimAnalysisAsync(
        [Description("Detailed reasoning for the claim analysis decision")] string reasoning,
        [Description(
            "Confidence score from 0-100 indicating how confident you are in this analysis"
        )]
            int confidenceScore
    )
    {
        try
        {
            if (confidenceScore < 0 || confidenceScore > 100)
            {
                return Task.FromResult("Error: Confidence score must be between 0 and 100.");
            }

            _claim.AgentReasoning = reasoning;
            _claim.AgentConfidenceScore = confidenceScore / 100m; // Convert to decimal 0-1
            _claim.AgentProcessedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Updated claim analysis for {ClaimId} with confidence score {ConfidenceScore}",
                _claim.Id,
                confidenceScore
            );

            return Task.FromResult(
                $"Successfully updated claim analysis with {confidenceScore}% confidence."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating claim analysis for claim {ClaimId}", _claim.Id);
            return Task.FromResult("Error updating claim analysis.");
        }
    }

    [KernelFunction]
    [Description("Set the fraud risk level for the claim")]
    public Task<string> SetFraudRiskAsync(
        [Description("Fraud risk level: Low, Medium, High, or Critical")] string riskLevel,
        [Description("List of specific fraud indicators found, separated by semicolons")]
            string fraudIndicators = ""
    )
    {
        try
        {
            if (!Enum.TryParse<FraudRiskLevel>(riskLevel, true, out var parsedRiskLevel))
            {
                return Task.FromResult(
                    "Error: Invalid risk level. Use Low, Medium, High, or Critical."
                );
            }

            _claim.FraudRiskLevel = parsedRiskLevel;
            _claim.IsFlagged = parsedRiskLevel >= FraudRiskLevel.High;

            if (!string.IsNullOrEmpty(fraudIndicators))
            {
                _claim.FraudIndicators = fraudIndicators
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Trim())
                    .ToList();
            }

            _logger.LogInformation(
                "Set fraud risk level to {RiskLevel} for claim {ClaimId}",
                riskLevel,
                _claim.Id
            );

            return Task.FromResult($"Successfully set fraud risk level to {riskLevel}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting fraud risk for claim {ClaimId}", _claim.Id);
            return Task.FromResult("Error setting fraud risk level.");
        }
    }

    [KernelFunction]
    [Description("Make a recommendation for claim approval or denial")]
    public Task<string> RecommendActionAsync(
        [Description("Recommended action: Approve, Deny, Pending, Review, or Investigate")]
            string action,
        [Description("Approved amount if recommending approval (leave empty for full amount)")]
            decimal? approvedAmount = null,
        [Description("Reason for denial if recommending denial")] string denialReason = ""
    )
    {
        try
        {
            // Validate the action
            var validActions = new[] { "Approve", "Deny", "Pending", "Review", "Investigate" };
            if (!validActions.Contains(action, StringComparer.OrdinalIgnoreCase))
            {
                return Task.FromResult(
                    $"Error: Invalid action. Use one of: {string.Join(", ", validActions)}"
                );
            }

            _claim.AgentRecommendation = action;

            switch (action.ToLower())
            {
                case "approve":
                    _claim.ApprovedAmount = approvedAmount ?? _claim.ClaimAmount;
                    _claim.Status = ClaimStatus.PendingReview; // Still needs human review
                    break;

                case "deny":
                    _claim.ApprovedAmount = 0;
                    _claim.RejectionReason = denialReason;
                    _claim.Status = ClaimStatus.PendingReview;
                    break;

                case "pending":
                    _claim.Status = ClaimStatus.RequestMoreInfo;
                    break;

                case "review":
                case "investigate":
                    _claim.RequiresHumanReview = true;
                    _claim.Status = ClaimStatus.PendingReview;
                    break;
            }

            _logger.LogInformation(
                "Recommended action {Action} for claim {ClaimId} with approved amount {ApprovedAmount}",
                action,
                _claim.Id,
                approvedAmount
            );

            return Task.FromResult($"Successfully recommended {action} for the claim.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making recommendation for claim {ClaimId}", _claim.Id);
            return Task.FromResult("Error making claim recommendation.");
        }
    }

    [KernelFunction]
    [Description("Flag the claim for human review with specific reasons")]
    public Task<string> FlagForReviewAsync(
        [Description("Reason why human review is required")] string reviewReason,
        [Description("Priority level: Low, Medium, High")] string priority = "Medium"
    )
    {
        try
        {
            _claim.RequiresHumanReview = true;
            _claim.Status = ClaimStatus.PendingReview;

            // Append review reason to existing reasoning
            var existingReasoning = _claim.AgentReasoning ?? "";
            _claim.AgentReasoning = string.IsNullOrEmpty(existingReasoning)
                ? $"FLAGGED FOR REVIEW: {reviewReason}"
                : $"{existingReasoning}\n\nFLAGGED FOR REVIEW: {reviewReason}";

            // Adjust fraud risk based on priority
            if (
                priority.Equals("High", StringComparison.OrdinalIgnoreCase)
                && _claim.FraudRiskLevel < FraudRiskLevel.High
            )
            {
                _claim.FraudRiskLevel = FraudRiskLevel.High;
                _claim.IsFlagged = true;
            }

            _logger.LogInformation(
                "Flagged claim {ClaimId} for {Priority} priority human review: {Reason}",
                _claim.Id,
                priority,
                reviewReason
            );

            return Task.FromResult(
                $"Successfully flagged claim for {priority.ToLower()} priority human review."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flagging claim {ClaimId} for review", _claim.Id);
            return Task.FromResult("Error flagging claim for review.");
        }
    }

    [KernelFunction]
    [Description("Add a note or comment to the claim for documentation purposes")]
    public Task<string> AddClaimNoteAsync(
        [Description("Note or comment to add to the claim")] string note
    )
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
            var formattedNote = $"[{timestamp} - AI Agent]: {note}";

            // Append to existing reasoning or create new
            var existingReasoning = _claim.AgentReasoning ?? "";
            _claim.AgentReasoning = string.IsNullOrEmpty(existingReasoning)
                ? formattedNote
                : $"{existingReasoning}\n\n{formattedNote}";

            _logger.LogDebug("Added note to claim {ClaimId}: {Note}", _claim.Id, note);

            return Task.FromResult("Successfully added note to claim.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding note to claim {ClaimId}", _claim.Id);
            return Task.FromResult("Error adding note to claim.");
        }
    }

    [KernelFunction]
    [Description("Get current claim information for reference during analysis")]
    public Task<string> GetClaimSummaryAsync()
    {
        try
        {
            var summary = $"""
                ## Claim Summary

                **Transaction Number:** {_claim.TransactionNumber}
                **Patient:** {_claim.PatientName}
                **Member:** {_claim.MembershipNumber}
                **Provider:** {_claim.ProviderPractice}
                **Claim Amount:** {_claim.Currency}{_claim.ClaimAmount:N2}
                **Status:** {_claim.Status}
                **Source:** {_claim.Source}
                **Submitted:** {_claim.SubmittedAt:yyyy-MM-dd}
                **Current Risk Level:** {_claim.FraudRiskLevel}
                **Flagged:** {(_claim.IsFlagged ? "Yes" : "No")}

                **Previous Analysis:** {_claim.AgentReasoning ?? "None"}
                **Previous Recommendation:** {_claim.AgentRecommendation ?? "None"}
                """;

            return Task.FromResult(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting claim summary for claim {ClaimId}", _claim.Id);
            return Task.FromResult("Error retrieving claim summary.");
        }
    }
}
