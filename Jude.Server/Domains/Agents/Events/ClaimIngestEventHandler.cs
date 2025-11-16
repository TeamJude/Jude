using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Agents.Workflows;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Domains.Agents.Events;

public interface IClaimIngestEventHandler
{
    Task HandleClaimIngestAsync(ClaimIngestEvent @event);
}

public class ClaimIngestEventHandler : IClaimIngestEventHandler
{
    private readonly JudeDbContext _dbContext;
    private readonly ILogger<ClaimIngestEventHandler> _logger;
    private readonly Orchestrator _orchestrator;

    public ClaimIngestEventHandler(
        JudeDbContext dbContext,
        ILogger<ClaimIngestEventHandler> logger,
        Orchestrator orchestrator
    )
    {
        _dbContext = dbContext;
        _logger = logger;
        _orchestrator = orchestrator;
    }

    public async Task HandleClaimIngestAsync(ClaimIngestEvent @event)
    {
        _logger.LogInformation(
            "Processing claim ingest event for Claim {ClaimId}",
            @event.Claim.Id
        );

        var existingClaim = await _dbContext.Claims.FirstOrDefaultAsync(c =>
            c.Id == @event.Claim.Id
        );

        if (existingClaim == null)
        {
            _logger.LogWarning(
                "Claim {ClaimId} not found in database. It may have been deleted or never inserted.",
                @event.Claim.Id
            );
            return;
        }

        if (existingClaim.Status != ClaimStatus.Pending)
        {
            _logger.LogDebug(
                "Claim {ClaimId} has status {Status}, skipping processing (already processed)",
                existingClaim.Id,
                existingClaim.Status
            );
            return;
        }

        _logger.LogInformation(
            "Claim {ClaimId} is Pending, triggering adjudication workflow",
            existingClaim.Id
        );

        await TriggerAdjudicationWorkflow(existingClaim);
    }

    private async Task TriggerAdjudicationWorkflow(ClaimModel claim)
    {
        _logger.LogInformation(
            "Triggering adjudication workflow for claim {ClaimId} - Amount: {Amount}",
            claim.Id,
            claim.TotalClaimAmount
        );

        try
        {
            // Use simplified orchestrator to process the claim
            var success = await _orchestrator.ProcessClaimAsync(claim);

            if (success)
            {
                _logger.LogInformation(
                    "Successfully processed claim {ClaimId} through orchestration",
                    claim.Id
                );
            }
            else
            {
                _logger.LogWarning("Orchestration processing failed for claim {ClaimId}", claim.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error in adjudication workflow for claim {ClaimId}: {Message}",
                claim.Id,
                ex.Message
            );

            // Mark claim as failed if processing fails
            claim.Status = ClaimStatus.Failed;
            await _dbContext.SaveChangesAsync();
        }
    }
}
