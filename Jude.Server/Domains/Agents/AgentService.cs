using System.Text.Json;
using Jude.Server.Data.Models;
using Jude.Server.Domains.Claims;
using Jude.Server.Domains.Fraud;
using Jude.Server.Domains.Policies;
using Jude.Server.Domains.Rules;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using OpenAI.Chat;

namespace Jude.Server.Domains.Agents;

public class AgentService
{
    private readonly IPolicyContext _policyContext;
    private readonly IRulesService _rulesService;
    private readonly IFraudService _fraudService;
    private readonly IClaimsService _claimsService;
    private readonly ILogger<AgentService> _logger;

    public AgentService(
        IPolicyContext policyContext,
        IRulesService rulesService,
        IFraudService fraudService,
        IClaimsService claimsService,
        ILogger<AgentService> logger
    )
    {
        _policyContext = policyContext;
        _rulesService = rulesService;
        _fraudService = fraudService;
        _claimsService = claimsService;
        _logger = logger;
    }

    public async Task<AgentReviewModel?> TestAgentAsync(string claimData, string? context = null)
    {
        try
        {
            // Build the kernel (no DB dependencies)
            var kernel = Kernel
                .CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                    Config.AppConfig.Azure.AI.ModelId,
                    Config.AppConfig.Azure.AI.Endpoint,
                    Config.AppConfig.Azure.AI.ApiKey
                )
                .Build();

            // Import only policy and pricing plugins (no decision plugin)
            var policyPlugin = new Plugins.PolicyPlugin(
                _policyContext,
                kernel.LoggerFactory.CreateLogger<Plugins.PolicyPlugin>()
            );
            var pricingPlugin = new Plugins.PricingPlugin(
                _claimsService,
                kernel.LoggerFactory.CreateLogger<Plugins.PricingPlugin>()
            );
            kernel.ImportPluginFromObject(policyPlugin, "Policy");
            kernel.ImportPluginFromObject(pricingPlugin, "Pricing");

            // Create JSON schema for AgentReviewModel
            var jsonSchema = """
                {
                    "type": "object",
                    "properties": {
                        "Decision": {
                            "type": "integer",
                            "enum": [1, 2],
                            "description": "1 for Approve, 2 for Reject"
                        },
                        "Recommendation": {
                            "type": "string",
                            "description": "Guidance for human reviewers"
                        },
                        "Reasoning": {
                            "type": "string",
                            "description": "Detailed justification for the decision"
                        },
                        "ConfidenceScore": {
                            "type": "number",
                            "minimum": 0.0,
                            "maximum": 1.0,
                            "description": "Confidence level in the decision"
                        }
                    },
                    "required": ["Decision", "Recommendation", "Reasoning", "ConfidenceScore"],
                    "additionalProperties": false
                }
                """;

            var chatResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "agent_review_result",
                jsonSchema: BinaryData.FromString(jsonSchema),
                jsonSchemaIsStrict: true
            );

            var agent = new ChatCompletionAgent()
            {
                Name = "Jude",
                Instructions = Prompts.AdjudicationEngine,
                Kernel = kernel,
                Arguments = new KernelArguments(
                    new AzureOpenAIPromptExecutionSettings()
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Required(),
                        MaxTokens = 16000,
                        ResponseFormat = chatResponseFormat,
                    }
                ),
            };

            // Build processing context (without caching)
            var processingContext = await BuildProcessingContextAsync();
            var fullContext = $"{processingContext}\n\n{context ?? string.Empty}";

            var initialMessage = new Microsoft.SemanticKernel.ChatMessageContent(
                role: AuthorRole.User,
                content: $"Please process this medical claim for adjudication (test mode, stateless):\n\n{claimData}\n\n{fullContext}\nAnalyze the claim and return your decision as a structured AgentReviewModel object."
            );

            AgentThread? thread = null;
            string? responseContent = null;
            await foreach (var response in agent.InvokeAsync(initialMessage, thread))
            {
                responseContent = response.Message.Content;
                thread = response.Thread;
            }

            // Parse the structured response as AgentReviewModel
            if (!string.IsNullOrWhiteSpace(responseContent))
            {
                try
                {
                    var review = JsonSerializer.Deserialize<AgentReviewModel>(responseContent);
                    if (review != null)
                    {
                        review.ReviewedAt = DateTime.UtcNow;
                        review.Id = Guid.NewGuid();
                    }
                    return review;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Could not parse agent response as AgentReviewModel. Raw response: {Response}",
                        responseContent
                    );
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running test agent");
            return null;
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
}
