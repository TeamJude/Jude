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
                if (
                    existingClaim.Status == ClaimStatus.Pending
                    && !existingClaim.AgentProcessedAt.HasValue
                )
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
        var patientName =
            $"{cimasData.Patient?.Personal?.FirstName} {cimasData.Patient?.Personal?.Surname}".Trim();

        // Calculate claim amount from products and services
        var claimAmount = CalculateClaimAmount(cimasData);

        // Determine initial fraud risk level based on amount and other factors
        var fraudRiskLevel = DetermineInitialFraudRisk(claimAmount, cimasData);

        var claimModel = new ClaimModel
        {
            Id = Guid.NewGuid(),
            TransactionNumber = @event.TransactionNumber,
            IngestedAt = @event.IngestedAt,
            UpdatedAt = @event.IngestedAt,
            Status = ClaimStatus.Pending,
            Source = Enum.Parse<ClaimSource>(@event.Source),
            SubmittedAt = DateTime.TryParse(
                cimasData.TransactionResponse?.DateTime,
                out var submittedDate
            )
                ? submittedDate
                : @event.IngestedAt,

            // Patient and Provider Info (extracted from CIMAS data)
            PatientName = patientName,
            MembershipNumber = cimasData.Member?.MedicalSchemeNumber.ToString() ?? "",
            ProviderPractice = "Unknown",

            // Financial Info
            ClaimAmount = claimAmount,
            Currency = cimasData.Member?.Currency ?? "USD",

            // Risk Assessment (initial)
            FraudRiskLevel = fraudRiskLevel,
            IsFlagged = fraudRiskLevel >= FraudRiskLevel.High,
            RequiresHumanReview = true, // All claims require human review initially

            // Store the full CIMAS payload for reference
            CIMASPayload = JsonSerializer.Serialize(cimasData),
        };

        return claimModel;
    }

    private decimal CalculateClaimAmount(ClaimResponse cimasData)
    {
        decimal totalAmount = 0;

        // Sum up product amounts
        if (cimasData.ProductResponse != null)
        {
            foreach (var product in cimasData.ProductResponse)
            {
                if (decimal.TryParse(product.TotalValues?.Claimed, out var productAmount))
                {
                    totalAmount += productAmount;
                }
            }
        }

        // Sum up service amounts
        if (cimasData.ServiceResponse != null)
        {
            foreach (var service in cimasData.ServiceResponse)
            {
                if (decimal.TryParse(service.TotalValues?.Claimed, out var serviceAmount))
                {
                    totalAmount += serviceAmount;
                }
            }
        }

        // Fallback to header total if individual items don't sum up
        if (
            totalAmount == 0
            && decimal.TryParse(
                cimasData.ClaimHeaderResponse?.TotalValues?.Claimed,
                out var headerAmount
            )
        )
        {
            totalAmount = headerAmount;
        }

        return totalAmount;
    }

    private FraudRiskLevel DetermineInitialFraudRisk(decimal claimAmount, ClaimResponse cimasData)
    {
        // Simple initial risk assessment - can be enhanced later
        if (claimAmount > 5000)
            return FraudRiskLevel.High;
        if (claimAmount > 2000)
            return FraudRiskLevel.Medium;

        // Check if CIMAS flagged it
        var isHeldForReview =
            cimasData.ClaimHeaderResponse?.ResponseCode?.Contains("HELD_FOR_REVIEW") == true;
        if (isHeldForReview)
            return FraudRiskLevel.Medium;

        return FraudRiskLevel.Low;
    }

    private async Task TriggerAdjudicationWorkflow(ClaimModel claim)
    {
        _logger.LogInformation(
            "Triggering adjudication workflow for claim {ClaimId} - Amount: {Amount} {Currency}",
            claim.Id,
            claim.ClaimAmount,
            claim.Currency
        );

        try
        {
            // Use simplified orchestrator to process the claim
            var success = await _orchestrator.ProcessClaimAsync(claim);

            if (success)
            {
                _logger.LogInformation(
                    "Successfully processed claim {ClaimId} through orchestration with recommendation: {Recommendation}",
                    claim.Id,
                    claim.AgentRecommendation ?? "None"
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

            // Ensure claim is flagged for review if agent processing fails
            claim.RequiresHumanReview = true;
            claim.FraudRiskLevel = FraudRiskLevel.Medium;
            claim.AgentReasoning = $"Adjudication workflow failed: {ex.Message}";
            claim.Status = ClaimStatus.Failed;
            await _dbContext.SaveChangesAsync();
        }
    }
}
