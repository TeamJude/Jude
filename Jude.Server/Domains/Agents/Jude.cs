using System.Text.Json;
using Jude.Server.Config;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Agents.Plugins;
using Jude.Server.Domains.Claims;
using Jude.Server.Domains.Policies;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Jude.Server.Domains.Agents;

public class Jude
{
    private readonly Kernel _kernel;
    private readonly IClaimsService _claimsService;
    private readonly IPolicyContext _policyContext;
    private readonly ILogger<Jude> _logger;

    public Jude(IClaimsService claimsService, IPolicyContext policyContext, ILogger<Jude> logger)
    {
        _claimsService = claimsService;
        _policyContext = policyContext;
        _logger = logger;

        _kernel = Kernel
            .CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                AppConfig.Azure.AI.ModelId,
                AppConfig.Azure.AI.Endpoint,
                AppConfig.Azure.AI.ApiKey
            )
            .Build();
    }

    public async Task<bool> ProcessClaimAsync(ClaimModel claim, string context)
    {
        try
        {
            _logger.LogInformation("Starting agent processing for claim {ClaimId}", claim.Id);

            var decisionPlugin = new DecisionPlugin(
                claim,
                _claimsService,
                _kernel.LoggerFactory.CreateLogger<DecisionPlugin>()
            );

            var policyPlugin = new PolicyPlugin(
                _policyContext,
                _kernel.LoggerFactory.CreateLogger<PolicyPlugin>()
            );

            var pricingPlugin = new PricingPlugin(
                _claimsService,
                _kernel.LoggerFactory.CreateLogger<PricingPlugin>()
            );

            _kernel.ImportPluginFromObject(decisionPlugin, "Decision");
            _kernel.ImportPluginFromObject(policyPlugin, "Policy");
            _kernel.ImportPluginFromObject(pricingPlugin, "Pricing");

            // Create the agent with explicit function calling requirements
            var agent = new ChatCompletionAgent()
            {
                Name = "Jude",
                Instructions = Prompts.AdjudicationEngine,
                Kernel = _kernel,
                Arguments = new KernelArguments(
                    new AzureOpenAIPromptExecutionSettings()
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Required(),
                        MaxTokens = 4000,
                    }
                ),
            };

            var claimData = JsonSerializer.Serialize(claim.Data);
            var initialMessage = new ChatMessageContent(
                role: AuthorRole.User,
                content: $"Please process this medical claim for adjudication:\n\n{claimData} \n {context}\nAnalyze the claim according to your instructions and make a decision using the MakeDecision function"
            );

            AgentThread? thread = null;
            var responseContent = "";

            await foreach (var response in agent.InvokeAsync(initialMessage, thread))
            {
                responseContent += response.Message.Content;
                thread = response.Thread;
            }

            _logger.LogDebug(
                "Agent response for claim {ClaimId}: {Response}",
                claim.Id,
                responseContent
            );

            _logger.LogInformation("Successfully processed claim {ClaimId}", claim.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing claim {ClaimId} with agent", claim.Id);
            return false;
        }
    }
}
