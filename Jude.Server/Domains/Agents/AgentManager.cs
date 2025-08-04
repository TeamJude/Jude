using Jude.Server.Data.Models;
using Jude.Server.Domains.Claims;
using Jude.Server.Domains.Fraud;
using Jude.Server.Domains.Rules;
using Microsoft.Extensions.Caching.Memory;

namespace Jude.Server.Domains.Agents;

public interface IAgentManager
{
    Task<bool> ProcessClaimAsync(ClaimModel claim);
}

public class AgentManager : IAgentManager
{
    private readonly Jude _jude;
    private readonly IRulesService _rulesService;
    private readonly IFraudService _fraudService;
    private readonly IMemoryCache _contextCache;
    private readonly ILogger<AgentManager> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private const string CONTEXT_CACHE_KEY = "processing_context";
    private static readonly TimeSpan ContextCacheExpiry = TimeSpan.FromMinutes(15);

    public AgentManager(
        Jude jude,
        IRulesService rulesService,
        IFraudService fraudService,
        IMemoryCache contextCache,
        ILogger<AgentManager> logger
    )
    {
        _jude = jude;
        _rulesService = rulesService;
        _fraudService = fraudService;
        _contextCache = contextCache;
        _logger = logger;

        // Subscribe to change events to invalidate cache
        _rulesService.OnRulesChanged += InvalidateContext;
        _fraudService.OnFraudIndicatorsChanged += InvalidateContext;
    }

    public async Task<bool> ProcessClaimAsync(ClaimModel claim)
    {
        _logger.LogInformation("Starting claim processing for claim {ClaimId}", claim.Id);

        await _semaphore.WaitAsync();
        try
        {
            // Get cached context or build new one
            var context = await GetCachedContextAsync();

            // Process the claim with the singleton agent
            var success = await _jude.ProcessClaimAsync(claim, context);

            if (success)
            {
                _logger.LogInformation(
                    "Successfully processed claim {ClaimId} with recommendation: {Recommendation}",
                    claim.Id,
                    claim.AgentRecommendation
                );
            }
            else
            {
                _logger.LogWarning("Agent processing failed for claim {ClaimId}", claim.Id);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing claim {ClaimId} with agent manager", claim.Id);

            // Ensure claim is flagged for human review on processing failure
            claim.RequiresHumanReview = true;
            claim.FraudRiskLevel = FraudRiskLevel.Medium;
            claim.Status = ClaimStatus.Failed;
            claim.AgentReasoningLog = [
                $"Agent processing failed: {ex.Message}"
            ];

            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<string> GetCachedContextAsync()
    {
        try
        {
            // Try to get context from cache first
            if (_contextCache.TryGetValue(CONTEXT_CACHE_KEY, out string cachedContext))
            {
                _logger.LogDebug("Using cached processing context");
                return cachedContext;
            }

            _logger.LogDebug("Building new processing context");
            var context = await BuildProcessingContextAsync();

            // Cache the context for future use
            var cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = ContextCacheExpiry,
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1), // Fallback absolute expiry
            };

            _contextCache.Set(CONTEXT_CACHE_KEY, context, cacheOptions);

            _logger.LogDebug(
                "Cached processing context for {ExpiryMinutes} minutes",
                ContextCacheExpiry.TotalMinutes
            );
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving processing context");
            return "Error retrieving context from the system.";
        }
    }

    private async Task<string> BuildProcessingContextAsync()
    {
        _logger.LogDebug("Building comprehensive context for claim processing");

        var context = "# Claim Processing Context\n\n";

        // Get active rules
        var activeRules = await _rulesService.GetActiveRules();

        context += "## Active Adjudication Rules\n\n";
        if (activeRules.Length != 0)
        {
            foreach (var rule in activeRules)
            {
                context += $"**{rule.Name}**\n";
                context += $"{rule.Description}\n\n";
            }
        }
        else
        {
            context += "No active rules found in the system.\n\n";
        }

        // Get fraud indicators
        var fraudIndicators = await _fraudService.GetActiveFraudIndicators();

        context += "## Active Fraud Detection Indicators\n\n";
        if (fraudIndicators.Length != 0)
        {
            foreach (var indicator in fraudIndicators)
            {
                context += $"**{indicator.Name}**\n";
                context += $"{indicator.Description}\n\n";
            }
        }
        else
        {
            context += "No active fraud indicators found in the system.\n\n";
        }

        // Add company policies
        context += GetCompanyPolicies();

        _logger.LogDebug(
            "Successfully built context with {RuleCount} rules and {IndicatorCount} fraud indicators",
            activeRules.Length,
            fraudIndicators.Length
        );

        return context;
    }

    private string GetCompanyPolicies()
    {
        return """
            ## Company Policies and Guidelines

            ### Maximum Coverage Limits
            - Annual maximum per member: $50,000
            - Single claim maximum: $10,000
            - Emergency services: Up to $25,000 per incident
            - Preventive care: 100% coverage up to $2,000 annually

            ### Pre-Authorization Requirements
            - Elective surgeries over $5,000
            - Specialized treatments and procedures
            - Experimental or investigational treatments
            - Durable medical equipment over $1,000

            ### Network Requirements
            - In-network providers: Standard coverage rates
            - Out-of-network providers: Reduced coverage (70% vs 90%)
            - Emergency services: Full coverage regardless of network status

            ### Documentation Requirements
            - Medical records for services over $1,000
            - Prior authorization for specified procedures
            - Diagnosis codes must support treatment
            - Provider credentials must be current

            ### Exclusions
            - Cosmetic procedures (unless medically necessary)
            - Experimental treatments not FDA approved
            - Services outside coverage period
            - Duplicate or overlapping services

            ### Billing Code Guidelines
            #### CPT Codes (Current Procedural Terminology)
            - 99000-99999: Evaluation and Management
            - 10000-69999: Surgery
            - 70000-79999: Radiology
            - 80000-89999: Pathology and Laboratory
            - 90000-99999: Medicine

            #### Typical Cost Ranges (USD)
            - Office visits: $150-$400
            - Basic lab work: $25-$200
            - X-rays: $100-$300
            - CT scans: $500-$1,500
            - MRI: $1,000-$3,000
            - Minor procedures: $200-$1,000
            - Major surgeries: $5,000-$50,000+

            """;
    }

    /// <summary>
    /// Invalidates the cached context, forcing a rebuild on next request
    /// Call this when rules or fraud indicators are updated
    /// </summary>
    public void InvalidateContext()
    {
        _contextCache.Remove(CONTEXT_CACHE_KEY);
        _logger.LogInformation("Processing context cache invalidated");
    }

    /// <summary>
    /// Gets the current cache status for monitoring purposes
    /// </summary>
    public bool IsContextCached => _contextCache.TryGetValue(CONTEXT_CACHE_KEY, out _);
}
