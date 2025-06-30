using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Claims;
using Jude.Server.Domains.Claims.Providers.CIMAS;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Jude.Server.Domains.Agents.Workflows;

public class ManualOrchestrationService
{
    private readonly AjudicationOrchestrator _orchestrator;
    private readonly JudeDbContext _dbContext;
    private readonly IClaimsService _claimsService;
    private readonly ILogger<ManualOrchestrationService> _logger;

    public ManualOrchestrationService(
        AjudicationOrchestrator orchestrator,
        JudeDbContext dbContext,
        IClaimsService claimsService,
        ILogger<ManualOrchestrationService> logger
    )
    {
        _orchestrator = orchestrator;
        _dbContext = dbContext;
        _claimsService = claimsService;
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

    public async Task<IngestAndProcessResult> IngestAndProcessRecentClaimsAsync(string practiceNumber, int maxClaims = 3)
    {
        _logger.LogInformation("Ingesting and processing {MaxClaims} recent claims from practice {PracticeNumber}", maxClaims, practiceNumber);

        var result = new IngestAndProcessResult();

        try
        {
            // Step 1: Get past claims from CIMAS
            _logger.LogInformation("Fetching past claims from CIMAS for practice {PracticeNumber}", practiceNumber);
            var pastClaimsResult = await _claimsService.GetPastClaimsAsync(practiceNumber);

            if (!pastClaimsResult.Success)
            {
                _logger.LogError("Failed to fetch past claims: {Errors}", string.Join(", ", pastClaimsResult.Errors));
                throw new InvalidOperationException($"Failed to fetch past claims: {string.Join(", ", pastClaimsResult.Errors)}");
            }

            // Step 2: Take the most recent claims (limit by maxClaims)
            var recentClaims = pastClaimsResult.Data
                .OrderByDescending(c => DateTime.TryParse(c.TransactionResponse?.DateTime, out var date) ? date : DateTime.MinValue)
                .Take(maxClaims)
                .ToList();

            _logger.LogInformation("Found {ClaimCount} recent claims to ingest", recentClaims.Count);

            // Step 3: Map and save claims to database
            var ingestedClaims = new List<ClaimModel>();
            foreach (var cimasResponse in recentClaims)
            {
                // Check if claim already exists
                var existingClaim = await _dbContext.Claims.FirstOrDefaultAsync(c =>
                    c.TransactionNumber == cimasResponse.TransactionResponse.Number);

                if (existingClaim != null)
                {
                    _logger.LogDebug("Claim {TransactionNumber} already exists, skipping ingestion", cimasResponse.TransactionResponse.Number);
                    continue;
                }

                // Map CIMAS response to ClaimModel
                var claimModel = MapCIMASResponseToClaimModel(cimasResponse);
                
                // Save to database
                _dbContext.Claims.Add(claimModel);
                ingestedClaims.Add(claimModel);
                
                _logger.LogInformation("Ingested claim {TransactionNumber} for patient {PatientName}", 
                    claimModel.TransactionNumber, claimModel.PatientName);
            }

            await _dbContext.SaveChangesAsync();
            result.IngestedClaims = ingestedClaims;

            // Step 4: Process ingested claims through orchestrator
            _logger.LogInformation("Processing {ClaimCount} ingested claims through orchestrator", ingestedClaims.Count);
            result.ProcessingResults = await _orchestrator.ProcessClaimsBatchAsync(ingestedClaims);

            _logger.LogInformation("Completed ingestion and processing. Ingested: {IngestedCount}, Processed: {ProcessedCount}",
                result.IngestedClaims.Count, result.ProcessingResults.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during ingestion and processing");
            throw;
        }
    }

    private ClaimModel MapCIMASResponseToClaimModel(ClaimResponse cimasResponse)
    {
        // Extract patient name
        var patientName = $"{cimasResponse.Patient?.Personal?.FirstName} {cimasResponse.Patient?.Personal?.Surname}".Trim();
        if (string.IsNullOrWhiteSpace(patientName))
        {
            patientName = "Unknown Patient";
        }

        // Calculate claim amount from products and services
        var claimAmount = CalculateClaimAmount(cimasResponse);

        // Determine initial fraud risk level
        var fraudRiskLevel = DetermineInitialFraudRisk(claimAmount, cimasResponse);

        // Parse submission date and ensure it's UTC
        var submittedAt = DateTime.TryParse(cimasResponse.TransactionResponse?.DateTime, out var submittedDate) 
            ? DateTime.SpecifyKind(submittedDate, DateTimeKind.Utc)
            : DateTime.UtcNow;

        return new ClaimModel
        {
            Id = Guid.NewGuid(),
            TransactionNumber = cimasResponse.TransactionResponse?.Number ?? Guid.NewGuid().ToString(),
            ClaimNumber = cimasResponse.TransactionResponse?.ClaimNumber ?? "",
            PatientName = patientName,
            MembershipNumber = cimasResponse.Member?.MedicalSchemeNumber.ToString() ?? "",
            ProviderPractice = "Demo Practice", // Would extract from CIMAS data if available
            ClaimAmount = claimAmount,
            Currency = cimasResponse.Member?.Currency ?? "USD",
            Status = ClaimStatus.Pending,
            Source = ClaimSource.CIMAS,
            IngestedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            SubmittedAt = submittedAt,
            FraudRiskLevel = fraudRiskLevel,
            IsFlagged = fraudRiskLevel >= FraudRiskLevel.High,
            RequiresHumanReview = true,
            CIMASPayload = JsonSerializer.Serialize(cimasResponse)
        };
    }

    private decimal CalculateClaimAmount(ClaimResponse cimasResponse)
    {
        decimal totalAmount = 0;

        // Sum up product amounts
        if (cimasResponse.ProductResponse != null)
        {
            foreach (var product in cimasResponse.ProductResponse)
            {
                if (decimal.TryParse(product.TotalValues?.Claimed, out var productAmount))
                {
                    totalAmount += productAmount;
                }
            }
        }

        // Sum up service amounts
        if (cimasResponse.ServiceResponse != null)
        {
            foreach (var service in cimasResponse.ServiceResponse)
            {
                if (decimal.TryParse(service.TotalValues?.Claimed, out var serviceAmount))
                {
                    totalAmount += serviceAmount;
                }
            }
        }

        // Fallback to header total if individual items don't sum up
        if (totalAmount == 0 && decimal.TryParse(cimasResponse.ClaimHeaderResponse?.TotalValues?.Claimed, out var headerAmount))
        {
            totalAmount = headerAmount;
        }

        return totalAmount;
    }

    private FraudRiskLevel DetermineInitialFraudRisk(decimal claimAmount, ClaimResponse cimasResponse)
    {
        // Simple initial risk assessment
        if (claimAmount > 5000) return FraudRiskLevel.High;
        if (claimAmount > 2000) return FraudRiskLevel.Medium;

        // Check if CIMAS flagged it
        var isHeldForReview = cimasResponse.ClaimHeaderResponse?.ResponseCode?.Contains("HELD_FOR_REVIEW") == true;
        if (isHeldForReview) return FraudRiskLevel.Medium;

        return FraudRiskLevel.Low;
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

public class IngestAndProcessResult
{
    public List<ClaimModel> IngestedClaims { get; set; } = new();
    public List<ClaimProcessingResult> ProcessingResults { get; set; } = new();
}
