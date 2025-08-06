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
                _logger.LogInformation("Successfully processed claim {ClaimId}", claim.Id);
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
            claim.Status = ClaimStatus.Failed;
            claim.UpdatedAt = DateTime.UtcNow;
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
            if (_contextCache.TryGetValue(CONTEXT_CACHE_KEY, out string? cachedContext))
            {
                _logger.LogDebug("Using cached processing context");
                return cachedContext!;
            }

            _logger.LogDebug("Building new processing context");
            var context = await BuildProcessingContextAsync();

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

        _logger.LogDebug(
            "Successfully built context with {RuleCount} rules and {IndicatorCount} fraud indicators",
            activeRules.Length,
            fraudIndicators.Length
        );

        return context;
    }

    public void InvalidateContext()
    {
        _contextCache.Remove(CONTEXT_CACHE_KEY);
        _logger.LogInformation("Processing context cache invalidated");
    }

    public bool IsContextCached => _contextCache.TryGetValue(CONTEXT_CACHE_KEY, out _);
}
