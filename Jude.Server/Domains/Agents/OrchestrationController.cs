using Jude.Server.Data.Models;
using Jude.Server.Domains.Agents.Workflows;
using Microsoft.AspNetCore.Mvc;

namespace Jude.Server.Domains.Agents;

[ApiController]
[Route("api/[controller]")]
public class OrchestrationController : ControllerBase
{
    private readonly ManualOrchestrationService _orchestrationService;
    private readonly ILogger<OrchestrationController> _logger;

    public OrchestrationController(
        ManualOrchestrationService orchestrationService,
        ILogger<OrchestrationController> logger)
    {
        _orchestrationService = orchestrationService;
        _logger = logger;
    }

    [HttpPost("process-claim/{claimId}")]
    public async Task<IActionResult> ProcessClaim(Guid claimId)
    {
        try
        {
            var result = await _orchestrationService.ProcessSingleClaimAsync(claimId);
            
            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    claimId = result.ClaimId,
                    recommendation = result.FinalRecommendation?.Recommendation,
                    confidence = result.FinalRecommendation?.ConsolidatedConfidence,
                    agentCount = result.AgentResults.Count,
                    processingTime = result.CompletedAt - result.StartedAt
                });
            }
            
            return BadRequest(new
            {
                success = false,
                claimId = result.ClaimId,
                error = result.FailureReason,
                processingTime = result.CompletedAt - result.StartedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing claim {ClaimId} via API", claimId);
            return StatusCode(500, new { error = "Internal server error during claim processing" });
        }
    }

    [HttpPost("process-batch/{status}")]
    public async Task<IActionResult> ProcessClaimsByStatus(string status, [FromQuery] int maxClaims = 10)
    {
        try
        {
            if (!Enum.TryParse<ClaimStatus>(status, true, out var claimStatus))
            {
                return BadRequest(new { error = "Invalid claim status" });
            }

            var results = await _orchestrationService.ProcessClaimsByStatusAsync(claimStatus, maxClaims);
            
            var summary = new
            {
                totalProcessed = results.Count,
                successful = results.Count(r => r.Success),
                failed = results.Count(r => !r.Success),
                averageProcessingTime = results.Any() 
                    ? results.Average(r => (r.CompletedAt - r.StartedAt)?.TotalMilliseconds ?? 0)
                    : 0,
                results = results.Select(r => new
                {
                    claimId = r.ClaimId,
                    success = r.Success,
                    recommendation = r.FinalRecommendation?.Recommendation,
                    confidence = r.FinalRecommendation?.ConsolidatedConfidence,
                    error = r.FailureReason
                })
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch with status {Status}", status);
            return StatusCode(500, new { error = "Internal server error during batch processing" });
        }
    }

    [HttpPost("reprocess-failed")]
    public async Task<IActionResult> ReprocessFailedClaims([FromQuery] int maxClaims = 5)
    {
        try
        {
            var results = await _orchestrationService.ReprocessFailedClaimsAsync(maxClaims);
            
            var summary = new
            {
                totalReprocessed = results.Count,
                successful = results.Count(r => r.Success),
                stillFailed = results.Count(r => !r.Success),
                results = results.Select(r => new
                {
                    claimId = r.ClaimId,
                    success = r.Success,
                    recommendation = r.FinalRecommendation?.Recommendation,
                    confidence = r.FinalRecommendation?.ConsolidatedConfidence,
                    error = r.FailureReason
                })
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reprocessing failed claims");
            return StatusCode(500, new { error = "Internal server error during reprocessing" });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetOrchestrationStats([FromQuery] DateTime? fromDate = null)
    {
        try
        {
            var stats = await _orchestrationService.GetOrchestrationStatsAsync(fromDate);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orchestration stats");
            return StatusCode(500, new { error = "Internal server error retrieving stats" });
        }
    }
} 