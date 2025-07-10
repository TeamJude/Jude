using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;

namespace Jude.Server.Domains.Agents.Workflows;

public class Orchestrator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Orchestrator> _logger;

    public Orchestrator(IServiceProvider serviceProvider, ILogger<Orchestrator> logger)
    {
        _serviceProvider = serviceProvider;
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

            // Create Jude agent and process the claim
            using var scope = _serviceProvider.CreateScope();
            var jude = scope.ServiceProvider.GetRequiredService<Jude>();

            var success = await jude.ProcessClaimAsync(claim);

            if (success)
            {
                _logger.LogInformation(
                    "Successfully processed claim {ClaimId} with recommendation: {Recommendation}",
                    claim.Id,
                    claim.AgentRecommendation
                );

                // Perform any post-processing tasks
                await PerformPostProcessing(claim);
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

    private async Task PerformPostProcessing(ClaimModel claim)
    {
        _logger.LogDebug("Performing post-processing for claim {ClaimId}", claim.Id);

        try
        {
            // Add any post-processing logic here, such as:
            // - Notifications
            // - Audit logging
            // - Integration with external systems
            // - Workflow triggers

            // For now, just log the completion
            _logger.LogInformation(
                "Post-processing completed for claim {ClaimId} - Status: {Status}, Recommendation: {Recommendation}",
                claim.Id,
                claim.Status,
                claim.AgentRecommendation
            );

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in post-processing for claim {ClaimId}", claim.Id);
            // Don't fail the entire process for post-processing errors
        }
    }

    private async Task UpdateClaimStatus(ClaimModel claim, ClaimStatus status, string note)
    {
        claim.Status = status;
        claim.UpdatedAt = DateTime.UtcNow;

        // Add note to reasoning
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
        var formattedNote = $"[{timestamp} - Orchestrator]: {note}";

        var existingReasoning = claim.AgentReasoning ?? "";
        claim.AgentReasoning = string.IsNullOrEmpty(existingReasoning)
            ? formattedNote
            : $"{existingReasoning}\n\n{formattedNote}";

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JudeDbContext>();
        dbContext.Claims.Update(claim);
        await dbContext.SaveChangesAsync();
    }
}
