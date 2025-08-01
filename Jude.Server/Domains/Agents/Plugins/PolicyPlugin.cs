using System.ComponentModel;
using Jude.Server.Domains.Policies;
using Microsoft.SemanticKernel;

namespace Jude.Server.Domains.Agents.Plugins;

public class PolicyPlugin
{
    private readonly IPolicyContext _policyContext;
    private readonly ILogger<PolicyPlugin> _logger;

    public PolicyPlugin(IPolicyContext policyContext, ILogger<PolicyPlugin> logger)
    {
        _policyContext = policyContext;
        _logger = logger;
    }

    [KernelFunction]
    [Description(
        "Ask questions about company policies, coverage guidelines, fraud indicators, billing rules, and adjudication criteria. Returns comprehensive policy information to inform claim decisions."
    )]
    public async Task<string> AskPolicy(
        [Description(
            "A specific question about company policies, coverage limits, fraud detection criteria, billing guidelines, or adjudication rules related to the claim being processed"
        )]
            string question
    )
    {
        try
        {
            _logger.LogInformation("Asking policy question: {Question}", question);

            var result = await _policyContext.AskPolicyAsync(question);

            if (result == null || string.IsNullOrWhiteSpace(result.Result))
            {
                _logger.LogWarning("No policy information found for question: {Question}", question);
                return "No relevant policy information found for this question. Please try rephrasing or ask about a different aspect.";
            }

            // Format the response with sources if available
            var response = $"**Policy Information:**\n{result.Result}";
            
            if (result.RelevantSources?.Any() == true)
            {
                response += "\n\n**Sources:**";
                foreach (var source in result.RelevantSources.Take(3)) // Limit to top 3 sources
                {
                    if (!string.IsNullOrWhiteSpace(source.SourceName))
                    {
                        response += $"\n- {source.SourceName}";
                        if (source.Partitions?.Any() == true)
                        {
                            var partition = source.Partitions.First();
                            if (!string.IsNullOrWhiteSpace(partition.Text))
                            {
                                // Add a brief snippet
                                var snippet = partition.Text.Length > 500 
                                    ? partition.Text[..500] + "..." 
                                    : partition.Text;
                                response += $" - {snippet}";
                            }
                        }
                    }
                }
            }

            _logger.LogInformation("Successfully retrieved policy information for question: {Question}", question);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving policy information for question: {Question}", question);
            return $"Error retrieving policy information: {ex.Message}. Please try asking a different question or contact support.";
        }
    }
}