using System.Text.Json;
using Jude.Server.Config;
using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using OpenAI.Chat;

namespace Jude.Server.Domains.Agents;

public class Jude
{
    private readonly Kernel _kernel;
    private readonly ILogger<Jude> _logger;

    public Jude(ILogger<Jude> logger)
    {
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

    public async Task<Result<AgentReviewModel>> ProcessClaimAsync(ClaimModel claim, string context)
    {
        _logger.LogInformation("Starting agent processing for claim {ClaimId}", claim.Id);

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
            Kernel = _kernel,
            Arguments = new KernelArguments(
                new AzureOpenAIPromptExecutionSettings()
                {
                    MaxTokens = 8000,
                    ResponseFormat = chatResponseFormat,
                }
            ),
        };

        var claimJson = JsonSerializer.Serialize(claim);
        var initialMessage = new Microsoft.SemanticKernel.ChatMessageContent(
            role: AuthorRole.User,
            content: $"Please process this medical claim for adjudication:\n\n{claimJson} \n {context}\n"
        );

        AgentThread? thread = null;
        var responseContent = "";

        await foreach (var response in agent.InvokeAsync(initialMessage, thread))
        {
            responseContent += response.Message.Content;
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
                return Result.Ok(review);
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

        return Result.Fail("failed to process claim");
    }
}
