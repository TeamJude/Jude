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

            var claimSummary = FormatClaimSummary(claim);
            var initialMessage = new ChatMessageContent(
                role: AuthorRole.User,
                content: $"Please process this medical claim for adjudication:\n\n{claimSummary}\n\n{context}\n\nAnalyze the claim according to your instructions and make a decision using the MakeDecision function. You MUST call MakeDecision to complete the processing."
            );

            AgentThread? thread = null;
            var responseContent = "";
            var decisionMade = false;

            await foreach (var response in agent.InvokeAsync(initialMessage, thread))
            {
                responseContent += response.Message.Content;
                thread = response.Thread;

                // Check if MakeDecision was called
                if (
                    response.Message.Content?.Contains("MakeDecision") == true
                    || response.Message.Content?.Contains("Decision recorded successfully") == true
                )
                {
                    decisionMade = true;
                }
            }

            _logger.LogDebug(
                "Agent response for claim {ClaimId}: {Response}",
                claim.Id,
                responseContent
            );

            // Verify that a decision was made
            if (!decisionMade)
            {
                _logger.LogWarning(
                    "Agent did not call MakeDecision for claim {ClaimId}. Forcing decision with default values.",
                    claim.Id
                );

                // Force a default decision if the agent didn't make one
                claim.AgentRecommendation = "REVIEW";
                claim.AgentReasoningLog =
                [
                    "Agent failed to make explicit decision - defaulting to review",
                ];
                claim.AgentConfidenceScore = 0.5m;
                claim.RequiresHumanReview = true;
                claim.AgentProcessedAt = DateTime.UtcNow;
                claim.Status = ClaimStatus.Review;
                claim.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _claimsService.UpdateClaimAsync(claim);
                if (!updateResult.Success)
                {
                    _logger.LogError(
                        "Failed to update claim {ClaimId} with default decision: {Error}",
                        claim.Id,
                        updateResult.Errors.FirstOrDefault()
                    );
                }
            }

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

            claim.AgentReasoningLog = [$"Agent processing failed: {ex.Message}"];
            claim.RequiresHumanReview = true;
            claim.FraudRiskLevel = FraudRiskLevel.Medium;
            claim.Status = ClaimStatus.Failed;

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

            here is the full claim payload :{claim.CIMASPayload}
            """;
    }
}
