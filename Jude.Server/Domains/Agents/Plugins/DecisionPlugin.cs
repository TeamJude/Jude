using System.ComponentModel;
using System.Text.Json;
using Jude.Server.Data.Models;
using Jude.Server.Domains.Claims;
using Microsoft.SemanticKernel;

namespace Jude.Server.Domains.Agents.Plugins;

public class DecisionPlugin
{
    private readonly ClaimModel _claim;
    private readonly IClaimsService _claimsService;
    private readonly ILogger<DecisionPlugin> _logger;

    public DecisionPlugin(
        ClaimModel claim,
        IClaimsService claimsService,
        ILogger<DecisionPlugin> logger
    )
    {
        _claim = claim;
        _claimsService = claimsService;
        _logger = logger;
    }

    [KernelFunction]
    [Description(
        "Make a decision for the claim by updating the recommendation, reasoning, confidence score, and other relevant fields. This function MUST be called to complete claim processing."
    )]
    public async Task<string> MakeDecision(
        [Description("Your final decision on the claim must be  either Approve | Reject")]
            string decision,
        [Description("Your justification and reasoning for your final decision")] string reasoning,
        [Description("The recommandation to the human reviewer on the claim")]
            string recommendation,
        [Description(
            "Confidence score how confident you are about your decision between 0.0 and 1.0"
        )]
            decimal confidenceScore = 0.0m
    )
    {
        try
        {
            _logger.LogInformation(
                "Making decision for claim {ClaimId}: {Recommendation}",
                _claim.Id,
                recommendation
            );

            var review = new AgentReviewModel()
            {
                ClaimId = _claim.Id,
                Decision = (ClaimDecision)Enum.Parse(typeof(ClaimDecision), decision),
                Reasoning = reasoning,
                Recommendation = recommendation,
                ConfidenceScore = confidenceScore,
                ReviewedAt = DateTime.UtcNow,
            };

            var result = await _claimsService.UpdateAgentReview(review);

            if (result.Success)
            {
                return "Claim processed successfully";
            }
            return "Something went wrong whilst making the decion";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making decision for claim {ClaimId}", _claim.Id);
            return $"Error recording decision: {ex.Message}";
        }
    }
}
