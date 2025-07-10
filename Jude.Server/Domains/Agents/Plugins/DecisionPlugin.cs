using System.ComponentModel;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Microsoft.SemanticKernel;

namespace Jude.Server.Domains.Agents.Plugins;

public class DecisionPlugin
{
    private readonly ClaimModel _claim;
    private readonly JudeDbContext _dbContext;
    private readonly ILogger<DecisionPlugin> _logger;

    public DecisionPlugin(ClaimModel claim, JudeDbContext dbContext, ILogger<DecisionPlugin> logger)
    {
        _claim = claim;
        _dbContext = dbContext;
        _logger = logger;
    }

    [KernelFunction]
    [Description(
        "Make a decision for the claim by updating the recommendation, reasoning, confidence score, and other relevant fields"
    )]
    public async Task<string> MakeDecision(
        [Description(
            "The recommendation for the claim (APPROVE, DENY, PENDING, REVIEW, INVESTIGATE)"
        )]
            string recommendation,
        [Description("Detailed reasoning for the decision")] string reasoning,
        [Description("Confidence score between 0.0 and 1.0")] decimal confidenceScore = 0.0m,
        [Description("Fraud risk level (Low, Medium, High, Critical)")]
            string fraudRiskLevel = "Low",
        [Description("Whether the claim requires human review (true/false)")]
            bool requiresHumanReview = false,
        [Description("Approved amount if different from claimed amount")]
            decimal? approvedAmount = null
    )
    {
        try
        {
            _logger.LogInformation(
                "Making decision for claim {ClaimId}: {Recommendation}",
                _claim.Id,
                recommendation
            );

            // Update claim with agent decision
            _claim.AgentRecommendation = recommendation.ToUpper();
            _claim.AgentReasoning = reasoning;
            _claim.AgentConfidenceScore = Math.Max(0.0m, Math.Min(1.0m, confidenceScore)); // Clamp between 0 and 1
            _claim.RequiresHumanReview = requiresHumanReview;
            _claim.AgentProcessedAt = DateTime.UtcNow;

            // Set approved amount if provided, or full amount for approvals
            if (approvedAmount.HasValue)
            {
                _claim.ApprovedAmount = approvedAmount.Value;
            }
            else if (recommendation.ToUpper() == "APPROVE")
            {
                _claim.ApprovedAmount = _claim.ClaimAmount;
            }

            // Parse and set fraud risk level
            if (Enum.TryParse<FraudRiskLevel>(fraudRiskLevel, true, out var riskLevel))
            {
                _claim.FraudRiskLevel = riskLevel;
            }

            // Update claim status based on recommendation
            _claim.Status = recommendation.ToUpper() switch
            {
                "APPROVE" => requiresHumanReview ? ClaimStatus.Review : ClaimStatus.Completed,
                "DENY" => ClaimStatus.Review, // Denials always require human review
                "PENDING" => ClaimStatus.Pending,
                "REVIEW" => ClaimStatus.Review,
                "INVESTIGATE" => ClaimStatus.Review,
                _ => ClaimStatus.Review,
            };

            // Set final decision for auto-approved claims
            if (_claim.Status == ClaimStatus.Completed && recommendation.ToUpper() == "APPROVE")
            {
                _claim.FinalDecision = ClaimDecision.Approved;
            }

            _claim.UpdatedAt = DateTime.UtcNow;

            // Save changes to database
            _dbContext.Claims.Update(_claim);
            await _dbContext.SaveChangesAsync();

            var result =
                $"Decision recorded successfully for claim {_claim.Id}:\n"
                + $"- Recommendation: {recommendation}\n"
                + $"- Confidence: {confidenceScore:P}\n"
                + $"- Status: {_claim.Status}\n"
                + $"- Requires Review: {requiresHumanReview}";

            if (approvedAmount.HasValue)
            {
                result += $"\n- Approved Amount: {approvedAmount:C}";
            }

            _logger.LogInformation("Successfully recorded decision for claim {ClaimId}", _claim.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making decision for claim {ClaimId}", _claim.Id);
            return $"Error recording decision: {ex.Message}";
        }
    }
}
