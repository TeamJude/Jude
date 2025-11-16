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
            if (claim.Status != ClaimStatus.Pending && claim.Status != ClaimStatus.Failed)
            {
                _logger.LogInformation(
                    "Claim {ClaimId} has already been processed, skipping",
                    claim.Id
                );
                return true;
            }

            await _claimsService.UpdateClaimStatus(claim.Id, ClaimStatus.UnderAgentReview);

            // Process the claim using the agent manager
            var result = await _agentManager.ProcessClaimAsync(claim);

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("Agent processing failed for claim {ClaimId}", claim.Id);
                await _claimsService.UpdateClaimStatus(claim.Id, ClaimStatus.UnderHumanReview);
                return false;
            }

            var agentReview = result.Data;

            // Save the agent review to the database
            var updateResult = await _claimsService.UpdateAgentReview(claim.Id, agentReview);

            if (!updateResult.Success)
            {
                _logger.LogError(
                    "Failed to save agent review for claim {ClaimId}",
                    claim.Id
                );
                await _claimsService.UpdateClaimStatus(claim.Id, ClaimStatus.Failed);
                return false;
            }

            _logger.LogInformation(
                "Successfully processed claim {ClaimId} with recommendation: {Recommendation}",
                claim.Id,
                agentReview.Recommendation
            );

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error in claim processing orchestration for claim {ClaimId}",
                claim.Id
            );
            await _claimsService.UpdateClaimStatus(claim.Id, ClaimStatus.Failed);
            return false;
        }
    }
}
