using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Domains.Agents.Workflows;

public class ManualOrchestrationService
{
    private readonly AjudicationOrchestrator _orchestrator;
    private readonly JudeDbContext _dbContext;
    private readonly ILogger<ManualOrchestrationService> _logger;

    public ManualOrchestrationService(
        AjudicationOrchestrator orchestrator,
        JudeDbContext dbContext,
        ILogger<ManualOrchestrationService> logger
    )
    {
        _orchestrator = orchestrator;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ClaimProcessingResult> ProcessSingleClaimAsync(Guid claimId)
    {
        _logger.LogInformation("Manual orchestration requested for claim {ClaimId}", claimId);

        var claim = await _dbContext.Claims.FirstOrDefaultAsync(c => c.Id == claimId);

        if (claim == null)
        {
            _logger.LogWarning("Claim {ClaimId} not found for manual processing", claimId);
            return new ClaimProcessingResult
            {
                ClaimId = claimId,
                Success = false,
                FailureReason = "Claim not found",
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
            };
        }

        return await _orchestrator.ProcessClaimAsync(claim);
    }

    public async Task<List<ClaimProcessingResult>> ProcessClaimsByStatusAsync(
        ClaimStatus status,
        int maxClaims = 10
    )
    {
        _logger.LogInformation(
            "Manual batch orchestration requested for claims with status {Status}",
            status
        );

        var claims = await _dbContext
            .Claims.Where(c => c.Status == status)
            .Take(maxClaims)
            .ToListAsync();

        if (!claims.Any())
        {
            _logger.LogInformation("No claims found with status {Status}", status);
            return new List<ClaimProcessingResult>();
        }

        _logger.LogInformation(
            "Processing {ClaimCount} claims with status {Status}",
            claims.Count,
            status
        );

        return await _orchestrator.ProcessClaimsBatchAsync(claims);
    }

    public async Task<List<ClaimProcessingResult>> ReprocessFailedClaimsAsync(int maxClaims = 5)
    {
        _logger.LogInformation("Reprocessing failed claims");

        var failedClaims = await _dbContext
            .Claims.Where(c =>
                c.RequiresHumanReview
                && c.AgentReasoning != null
                && c.AgentReasoning.Contains("failed")
            )
            .Take(maxClaims)
            .ToListAsync();

        if (!failedClaims.Any())
        {
            _logger.LogInformation("No failed claims found for reprocessing");
            return new List<ClaimProcessingResult>();
        }

        _logger.LogInformation("Reprocessing {ClaimCount} failed claims", failedClaims.Count);

        return await _orchestrator.ProcessClaimsBatchAsync(failedClaims);
    }

    public async Task<ClaimOrchestrationStats> GetOrchestrationStatsAsync(DateTime? fromDate = null)
    {
        fromDate ??= DateTime.UtcNow.AddDays(-7); // Default to last 7 days

        var totalClaims = await _dbContext.Claims.CountAsync(c => c.ProcessedAt >= fromDate);

        var processedClaims = await _dbContext.Claims.CountAsync(c =>
            c.ProcessedAt >= fromDate && c.AgentRecommendation != null
        );

        var approvedClaims = await _dbContext.Claims.CountAsync(c =>
            c.ProcessedAt >= fromDate && c.AgentRecommendation == "Approve"
        );

        var deniedClaims = await _dbContext.Claims.CountAsync(c =>
            c.ProcessedAt >= fromDate && c.AgentRecommendation == "Deny"
        );

        var reviewClaims = await _dbContext.Claims.CountAsync(c =>
            c.ProcessedAt >= fromDate && c.RequiresHumanReview
        );

        var avgConfidence = await _dbContext
            .Claims.Where(c => c.ProcessedAt >= fromDate && c.AgentConfidenceScore.HasValue)
            .AverageAsync(c => c.AgentConfidenceScore ?? 0);

        return new ClaimOrchestrationStats
        {
            FromDate = fromDate.Value,
            ToDate = DateTime.UtcNow,
            TotalClaims = totalClaims,
            ProcessedClaims = processedClaims,
            ApprovedClaims = approvedClaims,
            DeniedClaims = deniedClaims,
            ReviewClaims = reviewClaims,
            AverageConfidence = avgConfidence,
            ProcessingRate = totalClaims > 0 ? (decimal)processedClaims / totalClaims : 0,
        };
    }
}

public class ClaimOrchestrationStats
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalClaims { get; set; }
    public int ProcessedClaims { get; set; }
    public int ApprovedClaims { get; set; }
    public int DeniedClaims { get; set; }
    public int ReviewClaims { get; set; }
    public decimal AverageConfidence { get; set; }
    public decimal ProcessingRate { get; set; }
}
