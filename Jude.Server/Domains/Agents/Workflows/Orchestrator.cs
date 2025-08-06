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
            if (claim.Status != ClaimStatus.Pending || claim.Status != ClaimStatus.Failed)
            {
                _logger.LogInformation(
                    "Claim {ClaimId} has already been processed, skipping",
                    claim.Id
                );
                return true;
            }

            await _claimsService.UpdateClaimStatus(claim.Id, ClaimStatus.UnderAgentReview);

            // Process the claim using the agent manager
            var success = await _agentManager.ProcessClaimAsync(claim);

            if (success)
            {
                _logger.LogInformation(
                    "Successfully processed claim {ClaimId} with recommendation: {Recommendation}",
                    claim.Id,
                    claim.AgentReview?.Recommendation
                );
            }
            else
            {
                _logger.LogWarning("Agent processing failed for claim {ClaimId}", claim.Id);
                await _claimsService.UpdateClaimStatus(claim.Id, ClaimStatus.UnderHumanReview);
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
            await _claimsService.UpdateClaimStatus(claim.Id, ClaimStatus.Failed);
            return false;
        }
    }
}
