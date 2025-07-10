using System.ComponentModel;
using Jude.Server.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace Jude.Server.Domains.Agents.Plugins;

public class ContextPlugin
{
    private readonly JudeDbContext _dbContext;
    private readonly ILogger<ContextPlugin> _logger;

    public ContextPlugin(JudeDbContext dbContext, ILogger<ContextPlugin> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [KernelFunction]
    [Description(
        "Get comprehensive context for claim processing including active rules, fraud indicators, and company policies"
    )]
    public async Task<string> GetContext()
    {
        try
        {
            _logger.LogDebug("Retrieving comprehensive context for claim processing");

            var context = "# Claim Processing Context\n\n";

            // Get active rules
            var activeRules = await _dbContext
                .Rules.Where(r => r.Status == Data.Models.RuleStatus.Active)
                .OrderBy(r => r.Priority)
                .Select(r => new
                {
                    r.Name,
                    r.Description,
                    r.Priority,
                })
                .ToListAsync();

            context += "## Active Adjudication Rules\n\n";
            if (activeRules.Any())
            {
                foreach (var rule in activeRules)
                {
                    context += $"**{rule.Name}** (Priority: {rule.Priority})\n";
                    context += $"{rule.Description}\n\n";
                }
            }
            else
            {
                context += "No active rules found in the system.\n\n";
            }

            // Get fraud indicators
            var fraudIndicators = await _dbContext
                .FraudIndicators.Where(f => f.Status == Data.Models.IndicatorStatus.Active)
                .Select(f => new { f.Name, f.Description })
                .ToListAsync();

            context += "## Active Fraud Detection Indicators\n\n";
            if (fraudIndicators.Any())
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
                "Successfully retrieved context with {RuleCount} rules and {IndicatorCount} fraud indicators",
                activeRules.Count,
                fraudIndicators.Count
            );

            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving context for claim processing");
            return "Error retrieving context from the system.";
        }
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
}
