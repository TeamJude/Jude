using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jude.Server.Domains.Agents.Workflows;

public class AjudicationOrchestrator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AjudicationOrchestrator> _logger;

    public AjudicationOrchestrator(
        IServiceProvider serviceProvider,
        ILogger<AjudicationOrchestrator> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<ClaimProcessingResult> ProcessClaimAsync(ClaimModel claim)
    {
        _logger.LogInformation(
            "Starting claim adjudication orchestration for claim {ClaimId}",
            claim.Id
        );

        var result = new ClaimProcessingResult
        {
            ClaimId = claim.Id,
            StartedAt = DateTime.UtcNow,
            Success = false,
        };

        try
        {
            // Update claim status to indicate processing has started
            await UpdateClaimStatus(
                claim,
                ClaimStatus.Processing,
                "Adjudication orchestration started"
            );

            // Stage 1: Primary Adjudication Agent
            var primaryResult = await ExecutePrimaryAdjudication(claim);
            result.AgentResults.Add(primaryResult);

            if (!primaryResult.Success)
            {
                _logger.LogWarning("Primary adjudication failed for claim {ClaimId}", claim.Id);
                result.FailureReason = "Primary adjudication failed";
                return result;
            }

            // Stage 2: Specialized Agents (can run in parallel in the future)
            var specializedResults = await ExecuteSpecializedAgents(claim);
            result.AgentResults.AddRange(specializedResults);

            // Stage 3: Final Review and Consolidation
            var finalResult = await ConsolidateResults(claim, result.AgentResults);
            result.FinalRecommendation = finalResult;

            // Update final status based on consolidated results
            await ApplyFinalRecommendation(claim, finalResult);

            result.Success = true;
            result.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Successfully completed claim adjudication for {ClaimId} with recommendation: {Recommendation}",
                claim.Id,
                finalResult.Recommendation
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error during claim adjudication orchestration for claim {ClaimId}",
                claim.Id
            );

            result.Success = false;
            result.FailureReason = ex.Message;
            result.CompletedAt = DateTime.UtcNow;

            // Ensure claim is flagged for human review on orchestration failure
            await UpdateClaimStatus(
                claim,
                ClaimStatus.PendingReview,
                $"Orchestration failed: {ex.Message}"
            );
            claim.RequiresHumanReview = true;
            claim.FraudRiskLevel = FraudRiskLevel.Medium;

            return result;
        }
    }

    public async Task<List<ClaimProcessingResult>> ProcessClaimsBatchAsync(List<ClaimModel> claims)
    {
        _logger.LogInformation(
            "Starting batch adjudication orchestration for {ClaimCount} claims",
            claims.Count
        );

        var results = new List<ClaimProcessingResult>();
        var semaphore = new SemaphoreSlim(3, 3); // Limit concurrent processing

        var tasks = claims.Select(async claim =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await ProcessClaimAsync(claim);
            }
            finally
            {
                semaphore.Release();
            }
        });

        results.AddRange(await Task.WhenAll(tasks));

        var successCount = results.Count(r => r.Success);
        var failureCount = results.Count(r => !r.Success);

        _logger.LogInformation(
            "Batch adjudication completed. Success: {SuccessCount}, Failures: {FailureCount}",
            successCount,
            failureCount
        );

        return results;
    }

    private async Task<AgentResult> ExecutePrimaryAdjudication(ClaimModel claim)
    {
        _logger.LogDebug("Executing primary adjudication for claim {ClaimId}", claim.Id);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var adjudicator = scope.ServiceProvider.GetRequiredService<Ajudicator>();

            var success = await adjudicator.ProcessClaimAsync(claim);

            return new AgentResult
            {
                AgentType = "Primary Adjudicator",
                Success = success,
                ProcessedAt = DateTime.UtcNow,
                Recommendation = claim.AgentRecommendation,
                ConfidenceScore = claim.AgentConfidenceScore,
                Notes = success
                    ? "Primary adjudication completed successfully"
                    : "Primary adjudication failed",
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in primary adjudication for claim {ClaimId}", claim.Id);
            return new AgentResult
            {
                AgentType = "Primary Adjudicator",
                Success = false,
                ProcessedAt = DateTime.UtcNow,
                Notes = $"Primary adjudication error: {ex.Message}",
            };
        }
    }

    private async Task<List<AgentResult>> ExecuteSpecializedAgents(ClaimModel claim)
    {
        _logger.LogDebug("Executing specialized agents for claim {ClaimId}", claim.Id);

        var results = new List<AgentResult>();

        // TODO: Add specialized agents here
        // For now, we'll simulate with placeholder logic

        // Fraud Detection Specialist (future implementation)
        if (claim.FraudRiskLevel >= FraudRiskLevel.Medium)
        {
            var fraudResult = await ExecuteFraudDetectionAgent(claim);
            results.Add(fraudResult);
        }

        // Medical Necessity Reviewer (future implementation)
        if (claim.ClaimAmount > 1000)
        {
            var medicalResult = await ExecuteMedicalReviewAgent(claim);
            results.Add(medicalResult);
        }

        // Policy Compliance Checker (future implementation)
        var policyResult = await ExecutePolicyComplianceAgent(claim);
        results.Add(policyResult);

        return results;
    }

    private async Task<AgentResult> ExecuteFraudDetectionAgent(ClaimModel claim)
    {
        // Placeholder for future fraud detection agent
        await Task.Delay(50); // Simulate processing time

        return new AgentResult
        {
            AgentType = "Fraud Detection Specialist",
            Success = true,
            ProcessedAt = DateTime.UtcNow,
            Recommendation =
                claim.FraudRiskLevel >= FraudRiskLevel.High ? "Investigate" : "Approve",
            ConfidenceScore = 0.85m,
            Notes = $"Fraud risk assessment completed - Risk Level: {claim.FraudRiskLevel}",
        };
    }

    private async Task<AgentResult> ExecuteMedicalReviewAgent(ClaimModel claim)
    {
        // Placeholder for future medical review agent
        await Task.Delay(100); // Simulate processing time

        return new AgentResult
        {
            AgentType = "Medical Necessity Reviewer",
            Success = true,
            ProcessedAt = DateTime.UtcNow,
            Recommendation = "Approve",
            ConfidenceScore = 0.90m,
            Notes = "Medical necessity review completed - services appear appropriate",
        };
    }

    private async Task<AgentResult> ExecutePolicyComplianceAgent(ClaimModel claim)
    {
        // Placeholder for future policy compliance agent
        await Task.Delay(30); // Simulate processing time

        return new AgentResult
        {
            AgentType = "Policy Compliance Checker",
            Success = true,
            ProcessedAt = DateTime.UtcNow,
            Recommendation = "Approve",
            ConfidenceScore = 0.95m,
            Notes = "Policy compliance check completed - all requirements met",
        };
    }

    private async Task<ConsolidatedResult> ConsolidateResults(
        ClaimModel claim,
        List<AgentResult> agentResults
    )
    {
        _logger.LogDebug(
            "Consolidating results from {AgentCount} agents for claim {ClaimId}",
            agentResults.Count,
            claim.Id
        );

        var result = new ConsolidatedResult { ClaimId = claim.Id, ProcessedAt = DateTime.UtcNow };

        // Analyze all agent recommendations
        var recommendations = agentResults
            .Where(r => r.Success && !string.IsNullOrEmpty(r.Recommendation))
            .Select(r => r.Recommendation)
            .ToList();

        var avgConfidence = agentResults
            .Where(r => r.ConfidenceScore.HasValue)
            .Average(r => r.ConfidenceScore.Value);

        // Consolidation logic
        if (recommendations.Any(r => r.Equals("Investigate", StringComparison.OrdinalIgnoreCase)))
        {
            result.Recommendation = "Investigate";
            result.RequiresHumanReview = true;
        }
        else if (recommendations.Any(r => r.Equals("Deny", StringComparison.OrdinalIgnoreCase)))
        {
            result.Recommendation = "Deny";
            result.RequiresHumanReview = true;
        }
        else if (recommendations.Any(r => r.Equals("Review", StringComparison.OrdinalIgnoreCase)))
        {
            result.Recommendation = "Review";
            result.RequiresHumanReview = true;
        }
        else if (recommendations.All(r => r.Equals("Approve", StringComparison.OrdinalIgnoreCase)))
        {
            result.Recommendation = "Approve";
            result.RequiresHumanReview = avgConfidence < 0.80m; // Low confidence requires review
        }
        else
        {
            result.Recommendation = "Review";
            result.RequiresHumanReview = true;
        }

        result.ConsolidatedConfidence = avgConfidence;
        result.ConsolidationNotes =
            $"Consolidated from {agentResults.Count} agents. Average confidence: {avgConfidence:P}";

        await Task.CompletedTask; // For future async consolidation logic

        return result;
    }

    private async Task ApplyFinalRecommendation(
        ClaimModel claim,
        ConsolidatedResult consolidatedResult
    )
    {
        claim.AgentRecommendation = consolidatedResult.Recommendation;
        claim.RequiresHumanReview = consolidatedResult.RequiresHumanReview;
        claim.AgentConfidenceScore = consolidatedResult.ConsolidatedConfidence;

        // Update reasoning with consolidation notes
        var existingReasoning = claim.AgentReasoning ?? "";
        claim.AgentReasoning = string.IsNullOrEmpty(existingReasoning)
            ? consolidatedResult.ConsolidationNotes
            : $"{existingReasoning}\n\n{consolidatedResult.ConsolidationNotes}";

        // Set final status
        claim.Status = consolidatedResult.Recommendation switch
        {
            "Approve" => consolidatedResult.RequiresHumanReview
                ? ClaimStatus.PendingReview
                : ClaimStatus.Approved,
            "Deny" => ClaimStatus.PendingReview,
            "Investigate" => ClaimStatus.PendingReview,
            _ => ClaimStatus.PendingReview,
        };

        // Update processed timestamp
        claim.ProcessedAt = DateTime.UtcNow;

        // Save changes
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JudeDbContext>();
        dbContext.Claims.Update(claim);
        await dbContext.SaveChangesAsync();
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

// Result models for orchestration
public class ClaimProcessingResult
{
    public Guid ClaimId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public List<AgentResult> AgentResults { get; set; } = new();
    public ConsolidatedResult? FinalRecommendation { get; set; }
}

public class AgentResult
{
    public string AgentType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string? Recommendation { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public string? Notes { get; set; }
}

public class ConsolidatedResult
{
    public Guid ClaimId { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public bool RequiresHumanReview { get; set; }
    public decimal ConsolidatedConfidence { get; set; }
    public string ConsolidationNotes { get; set; } = string.Empty;
}
