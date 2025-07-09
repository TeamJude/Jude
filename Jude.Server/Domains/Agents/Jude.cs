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
            var contextPlugin = new GetContextPlugin(
                dbContext,
                scope.ServiceProvider.GetRequiredService<ILogger<GetContextPlugin>>()
            );
            var analysisPlugin = new ClaimAnalysisPlugin(
                claim,
                scope.ServiceProvider.GetRequiredService<ILogger<ClaimAnalysisPlugin>>()
            );

            // Import plugins into the kernel
            _kernel.ImportPluginFromObject(contextPlugin, "ClaimContext");
            _kernel.ImportPluginFromObject(analysisPlugin, "ClaimAnalysis");

            // Create the agent
            var agent = new ChatCompletionAgent()
            {
                Name = "Jude",
                Instructions = Prompts.AdjudicationEngine,
                Kernel = _kernel,
                Arguments = new KernelArguments(
                    new AzureOpenAIPromptExecutionSettings()
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Required(),
                    }
                ),
            };

            // Create the initial message with claim information
            var claimSummary = await analysisPlugin.GetClaimSummaryAsync();
            var initialMessage = new ChatMessageContent(
                role: AuthorRole.User,
                content: $"Please process this medical claim for adjudication:\n\n{claimSummary}\n\nAnalyze the claim according to your instructions and use the available tools to record your analysis and recommendation."
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

            claim.ProcessedAt = DateTime.UtcNow;

            // Save changes to the database
            await dbContext.SaveChangesAsync();

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

            return false;
        }
    }

    public async Task<bool> ProcessClaimsBatchAsync(List<ClaimModel> claims)
    {
        _logger.LogInformation("Starting batch processing of {ClaimCount} claims", claims.Count);

        var successCount = 0;
        var failureCount = 0;

        foreach (var claim in claims)
        {
            try
            {
                var success = await ProcessClaimAsync(claim);
                if (success)
                {
                    successCount++;
                }
                else
                {
                    failureCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch processing claim {ClaimId}", claim.Id);
                failureCount++;
            }

            // Add a small delay between claims to avoid overwhelming the AI service
            await Task.Delay(100);
        }

        _logger.LogInformation(
            "Batch processing completed. Success: {SuccessCount}, Failures: {FailureCount}",
            successCount,
            failureCount
        );

        return failureCount == 0;
    }
}
