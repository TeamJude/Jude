using Jude.Server.Data.Models;
using Jude.Server.Domains.Claims;

namespace Jude.Server.Domains.Agents.Workflows;

public class Orchestrator
{
    private readonly IAgentManager _agentManager;
    private readonly IClaimsService _claimsService;
    private readonly ILogger<Orchestrator> _logger;

    public Orchestrator(
        IAgentManager agentManager,
        IClaimsService claimsService,
        ILogger<Orchestrator> logger
    )
    {
        _agentManager = agentManager;
        _claimsService = claimsService;
        _logger = logger;
    }

    public async Task<bool> ProcessClaimAsync(ClaimModel claim)
    {
        _logger.LogInformation(
            "Starting claim processing orchestration for claim {ClaimId}",
            claim.Id
        );

        try
        {
            // Check if claim has already been processed
            if (claim.Status != ClaimStatus.Pending && claim.AgentProcessedAt.HasValue)
            {
                _logger.LogInformation(
                    "Claim {ClaimId} has already been processed, skipping",
                    claim.Id
                );
                return true;
            }

            // Update claim status to processing
            await UpdateClaimStatus(claim, ClaimStatus.Processing, "Agent processing started");

            // Process the claim using the agent manager
            var success = await _agentManager.ProcessClaimAsync(claim);

            if (success)
            {
                _logger.LogInformation(
                    "Successfully processed claim {ClaimId} with recommendation: {Recommendation}",
                    claim.Id,
                    claim.AgentRecommendation
                );
            }
            else
            {
                _logger.LogWarning("Agent processing failed for claim {ClaimId}", claim.Id);
                await UpdateClaimStatus(
                    claim,
                    ClaimStatus.Review,
                    "Agent processing failed - requires human review"
                );
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error in claim processing orchestration for claim {ClaimId}",
                claim.Id
            );

            // Ensure claim is flagged for human review on orchestration failure
            await UpdateClaimStatus(
                claim,
                ClaimStatus.Review,
                $"Orchestration failed: {ex.Message}"
            );
            claim.RequiresHumanReview = true;
            claim.FraudRiskLevel = FraudRiskLevel.Medium;

            return false;
        }
    }

    private async Task UpdateClaimStatus(ClaimModel claim, ClaimStatus status, string note)
    {
        claim.Status = status;
        claim.UpdatedAt = DateTime.UtcNow;

        // Add note to reasoning
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
        var formattedNote = $"[{timestamp} - Orchestrator]: {note}";

        var existingReasoning = claim.AgentReasoningLog ?? [];
        claim.AgentReasoningLog = existingReasoning.Count == 0
            ? [formattedNote]
            : [..existingReasoning, formattedNote];

        await _claimsService.UpdateClaimAsync(claim);
    }
}
