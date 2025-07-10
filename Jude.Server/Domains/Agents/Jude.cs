using Jude.Server.Config;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Agents.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Jude.Server.Domains.Agents;

public class Jude
{
    private readonly Kernel _kernel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Jude> _logger;

    public Jude(IServiceProvider serviceProvider, ILogger<Jude> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _kernel = Kernel
            .CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                AppConfig.AzureAI.ModelId,
                AppConfig.AzureAI.Endpoint,
                AppConfig.AzureAI.ApiKey
            )
            .Build();
    }

    public async Task<bool> ProcessClaimAsync(ClaimModel claim)
    {
        try
        {
            _logger.LogInformation("Starting agent processing for claim {ClaimId}", claim.Id);

            // Create a new scope for this claim processing
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<JudeDbContext>();

            // Create plugins with scoped dependencies
            var contextPlugin = new ContextPlugin(
                dbContext,
                scope.ServiceProvider.GetRequiredService<ILogger<ContextPlugin>>()
            );
            var decisionPlugin = new DecisionPlugin(
                claim,
                dbContext,
                scope.ServiceProvider.GetRequiredService<ILogger<DecisionPlugin>>()
            );
            // Import plugins into the kernel
            _kernel.ImportPluginFromObject(contextPlugin, "Context");
            _kernel.ImportPluginFromObject(decisionPlugin, "Decision");

            // Create the agent
            var agent = new ChatCompletionAgent()
            {
                Name = "Jude",
                Instructions = Prompts.AdjudicationEngine,
                Kernel = _kernel,
                Arguments = new KernelArguments(
                    new AzureOpenAIPromptExecutionSettings()
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                    }
                ),
            };

            // Create the initial message with claim information
            var claimSummary = FormatClaimSummary(claim);
            var initialMessage = new ChatMessageContent(
                role: AuthorRole.User,
                content: $"Please process this medical claim for adjudication:\n\n{claimSummary}\n\nFirst get the context using the available tools, then analyze the claim according to your instructions and make a decision using the MakeDecision function."
            );

            // Process the claim through the agent
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

            _logger.LogInformation(
                "Successfully processed claim {ClaimId} with recommendation: {Recommendation}",
                claim.Id,
                claim.AgentRecommendation ?? "None"
            );

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing claim {ClaimId} with agent", claim.Id);

            // Mark the claim as having an error
            claim.AgentReasoning = $"Agent processing failed: {ex.Message}";
            claim.RequiresHumanReview = true;
            claim.FraudRiskLevel = FraudRiskLevel.Medium; // Escalate due to processing error
            claim.Status = ClaimStatus.Review;

            return false;
        }
    }

    private string FormatClaimSummary(ClaimModel claim)
    {
        return $"""
            ## Claim Summary

            **Claim ID:** {claim.Id}
            **Transaction Number:** {claim.TransactionNumber}
            **Patient Name:** {claim.PatientName}
            **Membership Number:** {claim.MembershipNumber}
            **Provider Practice:** {claim.ProviderPractice}
            **Claim Amount:** {claim.ClaimAmount:C} {claim.Currency}
            **Submitted Date:** {claim.SubmittedAt?.ToString("yyyy-MM-dd") ?? "Unknown"}
            **Source:** {claim.Source}
            **Current Status:** {claim.Status}
            **Initial Fraud Risk:** {claim.FraudRiskLevel}

            **Additional Details:**
            - Ingested At: {claim.IngestedAt:yyyy-MM-dd HH:mm:ss}
            - Requires Human Review: {claim.RequiresHumanReview}
            """;
    }
}
