using System.ComponentModel;
using Jude.Server.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace Jude.Server.Domains.Agents.Plugins;

public class GetContextPlugin
{
    private readonly JudeDbContext _dbContext;
    private readonly ILogger<GetContextPlugin> _logger;

    public GetContextPlugin(JudeDbContext dbContext, ILogger<GetContextPlugin> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [KernelFunction]
    [Description("Get all active adjudication rules that should be applied when processing claims")]
    public async Task<string> GetActiveRulesAsync()
    {
        try
        {
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

            if (!activeRules.Any())
            {
                return "No active rules found in the system.";
            }

            var rulesText = "## Active Adjudication Rules\n\n";
            foreach (var rule in activeRules)
            {
                rulesText += $"**{rule.Name}** (Priority: {rule.Priority})\n";
                rulesText += $"{rule.Description}\n\n";
            }

            _logger.LogDebug(
                "Retrieved {RuleCount} active rules for agent context",
                activeRules.Count
            );
            return rulesText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active rules for agent context");
            return "Error retrieving active rules from the system.";
        }
    }

    [KernelFunction]
    [Description(
        "Get all active fraud indicators and detection criteria to help identify potentially fraudulent claims"
    )]
    public async Task<string> GetFraudIndicatorsAsync()
    {
        try
        {
            var fraudIndicators = await _dbContext
                .FraudIndicators.Where(f => f.Status == Data.Models.IndicatorStatus.Active)
                .Select(f => new { f.Name, f.Description })
                .ToListAsync();

            if (!fraudIndicators.Any())
            {
                return "No active fraud indicators found in the system.";
            }

            var indicatorsText = "## Active Fraud Detection Indicators\n\n";
            foreach (var indicator in fraudIndicators)
            {
                indicatorsText += $"**{indicator.Name}**\n";
                indicatorsText += $"{indicator.Description}\n\n";
            }

            _logger.LogDebug(
                "Retrieved {IndicatorCount} fraud indicators for agent context",
                fraudIndicators.Count
            );
            return indicatorsText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fraud indicators for agent context");
            return "Error retrieving fraud indicators from the system.";
        }
    }

    [KernelFunction]
    [Description("Get general policy guidelines and coverage limits for claim adjudication")]
    public string GetPolicyGuidelinesAsync()
    {
        // This could be enhanced to pull from a policy database
        // For now, providing basic guidelines that would typically be configured


        var guidelines = """
            ## Policy Guidelines and Coverage Limits

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
            """;

        _logger.LogDebug("Provided policy guidelines for agent context");
        return guidelines;
    }

    [KernelFunction]
    [Description(
        "Get information about common billing codes and their typical costs for comparison"
    )]
    public async Task<string> GetBillingCodeReferenceAsync(
        [Description("The medical billing code to look up")] string code = ""
    )
    {
        // This would typically connect to a medical coding database
        // For now, providing a basic framework

        await Task.CompletedTask; // Placeholder for potential future database call

        if (string.IsNullOrEmpty(code))
        {
            return """
                ## Common Billing Code Categories

                ### CPT Codes (Current Procedural Terminology)
                - 99000-99999: Evaluation and Management
                - 10000-69999: Surgery
                - 70000-79999: Radiology
                - 80000-89999: Pathology and Laboratory
                - 90000-99999: Medicine

                ### ICD-10 Diagnosis Codes
                - Medical necessity must align with diagnosis
                - Chronic conditions require ongoing documentation
                - Acute conditions should show resolution or treatment progression

                ### Typical Cost Ranges (USD)
                - Office visits: $150-$400
                - Basic lab work: $25-$200
                - X-rays: $100-$300
                - CT scans: $500-$1,500
                - MRI: $1,000-$3,000
                - Minor procedures: $200-$1,000
                - Major surgeries: $5,000-$50,000+
                """;
        }

        // In a real implementation, this would query a medical coding database
        return $"Billing code {code} information would be retrieved from medical coding database.";
    }
}
