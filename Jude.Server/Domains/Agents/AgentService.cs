using System.Text.Json;
using Jude.Server.Data.Models;
using Jude.Server.Domains.Claims;
using Jude.Server.Domains.Fraud;
using Jude.Server.Domains.Policies;
using Jude.Server.Domains.Rules;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using OpenAI.Chat;
using Azure.Storage.Blobs;
using System.Net.Http;
using Jude.Server.Config;

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
            // Build the kernel with OpenAI chat completion
            var kernel = Kernel
                .CreateBuilder()
                // .AddAzureOpenAIChatCompletion(
                //     Config.AppConfig.Azure.AI.ModelId,
                //     Config.AppConfig.Azure.AI.Endpoint,
                //     Config.AppConfig.Azure.AI.ApiKey
                // )
                .AddOpenAIChatCompletion(
                    modelId: "gpt-4.1",
                    apiKey:AppConfig.OpenAI.ApiKey
                )
                .Build();

            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            // Get policy PDF bytes
            var policyBytes = await GetPolicyPdfBytesAsync();
            if (policyBytes == null || policyBytes.Length == 0)
            {
                _logger.LogWarning("No policy PDF found, proceeding without policy context");
            }

            // Build processing context
            var processingContext = await BuildProcessingContextAsync();
            var fullContext = $"{processingContext}\n\n{context ?? string.Empty}";

            // Create chat history
            var history = new ChatHistory();
            history.AddSystemMessage(GetSystemPrompt());

            // Create user content with policy PDF if available
            var userContent = CreateUserContent(claimData, fullContext, policyBytes);
            history.AddUserMessage(userContent);




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

            // Create execution settings
            var executionSettings = new AzureOpenAIPromptExecutionSettings
            {
                MaxTokens = 16000,
                ResponseFormat = chatResponseFormat,
            };

            // Get response
            var response = await chatService.GetChatMessageContentAsync(history, executionSettings);
            var responseContent = response.Content;

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

    private async Task<byte[]?> GetPolicyPdfBytesAsync()
    {
        try
        {
            var policyUrl = "https://judestore.blob.core.windows.net/policies/policy_index/7b0131003a8b4a6a80cd60be91f99470202508071105519294434/main_policy_e6d9f03a-2756-4504-80ef-31d98063717b.pdf";
            
            _logger.LogInformation("Downloading policy PDF from: {Url}", policyUrl);
            
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(policyUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to download policy PDF. Status: {Status}", response.StatusCode);
                return null;
            }
            
            var pdfBytes = await response.Content.ReadAsByteArrayAsync();
            _logger.LogInformation("Successfully downloaded policy PDF. Size: {Size} bytes", pdfBytes.Length);
            
            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving policy PDF");
            return null;
        }
    }

    #pragma warning disable SKEXP0001 
    private ChatMessageContentItemCollection CreateUserContent(string claimData, string context, byte[]? policyBytes)
    {
        var textContent = new TextContent($"Please process this medical claim for adjudication:\n\nClaim Data: {claimData}\n\nContext: {context}");

        if (policyBytes != null && policyBytes.Length > 0)
        {
            return [textContent, new BinaryContent(policyBytes, "application/pdf")];
        }

        return [textContent];
    }

    private string GetSystemPrompt()
    {
        return @"You are Jude, an expert medical claims adjudication AI system. Your role is to analyze medical claims and provide structured decisions.

You can process both text-based claim data and PDF policy documents. When a policy PDF is provided, use it as the primary reference for adjudication rules and guidelines.

IMPORTANT: You must respond with ONLY a valid JSON object. Do not include any other text, explanations, or formatting.

Your response must be a valid JSON object with the following structure:
{
    ""Decision"": 1 or 2 (1 for Approve, 2 for Reject),
    ""Recommendation"": ""Markdown-formatted guidance for human reviewers with policy citations"",
    ""Reasoning"": ""Detailed markdown-formatted justification with specific policy references"",
    ""ConfidenceScore"": 0.0 to 1.0 (confidence level in the decision)
}

Key guidelines:
- Analyze the claim data thoroughly
- When a policy PDF is provided, cite specific sections, clauses, or rules from the policy document
- Use markdown formatting for better readability:
  - Use **bold** for important points and section headers
  - Use bullet points for lists
  - Use > for direct policy quotes
  - Use `code` for specific policy references (e.g., `Section 3.2`, `Clause 5.1`)
- Structure your reasoning with clear sections:
  - **Policy Analysis**: What specific policy rules apply to this claim
  - **Claim Assessment**: How the claim meets or fails policy requirements
  - **Risk Factors**: Any concerns, fraud indicators, or billing irregularities
  - **Recommendation**: Clear guidance for human reviewers with specific next steps
- When citing policy sections, use format: `Section X.Y` or `Clause X.Y.Z`
- Always explain WHY a policy section is relevant to the decision
- Consider medical necessity, coverage rules, and billing accuracy
- Flag potential fraud or billing irregularities
- Provide clear reasoning for your decision
- Be conservative with confidence scores
- If uncertain, recommend human review
- Always reference specific policy sections when making decisions
- Return ONLY the JSON object, no additional text";
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
