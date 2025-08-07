using System.Globalization;
using System.Text.Json;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Agents.Workflows;
using Jude.Server.Domains.Claims.Providers.CIMAS;
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
            "Processing claim ingest event for transaction {TransactionNumber}",
            @event.TransactionNumber
        );

        try
        {
            // Check if claim already exists to avoid duplicates
            var existingClaim = await _dbContext.Claims.FirstOrDefaultAsync(c =>
                c.TransactionNumber == @event.TransactionNumber
            );

            if (existingClaim != null)
            {
                _logger.LogDebug(
                    "Claim with transaction number {TransactionNumber} already exists with status {Status}, checking if processing needed",
                    @event.TransactionNumber,
                    existingClaim.Status
                );

                // If claim exists but hasn't been processed yet, trigger processing
                if (existingClaim.Status == ClaimStatus.Pending)
                {
                    _logger.LogInformation(
                        "Triggering processing for existing pending claim {ClaimId}",
                        existingClaim.Id
                    );
                    await TriggerAdjudicationWorkflow(existingClaim);
                }
                return;
            }

            // Map CIMAS claim data to our internal model
            var claimModel = MapCIMASClaimToModel(@event);

            // Save to database
            _dbContext.Claims.Add(claimModel);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully ingested claim {ClaimId} with transaction number {TransactionNumber}",
                claimModel.Id,
                @event.TransactionNumber
            );

            // Trigger adjudication workflow
            await TriggerAdjudicationWorkflow(claimModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing claim ingest event for transaction {TransactionNumber}: {Message}",
                @event.TransactionNumber,
                ex.Message
            );
            throw;
        }
    }

    private ClaimModel MapCIMASClaimToModel(ClaimIngestEvent @event)
    {
        var cimasData = @event.CIMASClaimData;

        // Extract patient name from CIMAS data
        var patientFirstName = cimasData.Patient?.Personal?.FirstName ?? "";
        var patientSurname = cimasData.Patient?.Personal?.Surname ?? "";

        // Calculate total claim amount from services
        var totalClaimAmount = decimal.Parse(
            @event.CIMASClaimData.ClaimHeaderResponse.TotalValues.Claimed,
            NumberStyles.Any,
            CultureInfo.InvariantCulture
        );

        // Extract claim number from transaction response
        var claimNumber = cimasData.TransactionResponse?.ClaimNumber ?? "";

        // Extract medical scheme name
        var medicalSchemeName = cimasData.Member?.MedicalSchemeName ?? "";

        var claimModel = new ClaimModel
        {
            TransactionNumber = @event.TransactionNumber,
            ClaimNumber = claimNumber,
            PatientFirstName = patientFirstName,
            PatientSurname = patientSurname,
            MedicalSchemeName = medicalSchemeName,
            TotalClaimAmount = totalClaimAmount,
            IngestedAt = @event.IngestedAt,
            UpdatedAt = @event.IngestedAt,
            Status = ClaimStatus.Pending,
            Data = cimasData
        };

        return claimModel;
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
