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
using Microsoft.AspNetCore.Http;

namespace Jude.Server.Domains.Agents;

public class AgentService
{
    private readonly IPolicyContext _policyContext;
    private readonly IRulesService _rulesService;
    private readonly IFraudService _fraudService;
    private readonly IClaimsService _claimsService;
    private readonly ILogger<AgentService> _logger;
    private readonly IChatCompletionService _extractionChatService;
    private readonly IChatCompletionService _reviewChatService;

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

        // Build kernels and chat services once in constructor
        var extractionKernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-4.1",
                apiKey: AppConfig.OpenAI.ApiKey
            )
            .Build();

        var reviewKernel = Kernel
            .CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                AppConfig.Azure.AI.ModelId,
                AppConfig.Azure.AI.Endpoint,
                AppConfig.Azure.AI.ApiKey
            )
            .Build();

        _extractionChatService = extractionKernel.GetRequiredService<IChatCompletionService>();
        _reviewChatService = reviewKernel.GetRequiredService<IChatCompletionService>();
    }
#pragma warning disable SKEXP0001 
    public async Task<string> ExtractClaimDataAsync(IFormFile file)
    {
        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var fileBytes = stream.ToArray();

            var fileContent = new BinaryContent(fileBytes, "application/pdf");

            var history = new ChatHistory("Extract the captured claim data from the uploaded PDF file and present the patient and claimant details in a structured markdown format.");
            history.AddUserMessage([fileContent]);

            var response = await _extractionChatService.GetChatMessageContentAsync(history, new AzureOpenAIPromptExecutionSettings { MaxTokens = 16000 });

            var responseContent = response.Content;


            // Placeholder extraction result in markdown
            var markdown = $$"""
# Extracted Claim Data (Preview)

- **Filename**: {file.FileName}
- **Size**: {fileSizeKb:F1} KB
- **Processed At**: {DateTime.UtcNow:O}

## Patient
- Name: Jane Doe
- Policy Number: P-123456
- Date of Birth: 1990-01-01

## Provider
- Facility: City Health Clinic
- Provider ID: PRV-98765

## Encounter
- Admission Date: 2025-07-01
- Discharge Date: 2025-07-02
- Diagnosis Codes:
  - ICD-10: Z00.00 (General adult medical examination)
s
## Line Items
1. CPT 99213 — Office/outpatient visit established patient — Qty: 1 — Amount: $120.00
2. CPT 81001 — Urinalysis automated — Qty: 1 — Amount: $20.00

## Notes
> Placeholder: AI-powered PDF form extraction will populate real values here.
""";

            return responseContent ?? markdown;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting claim data from uploaded PDF");
            return "# Extraction Failed\n\nAn error occurred while extracting data from the uploaded PDF.";
        }
    }

    public async Task<AgentReviewModel?> TestAgentAsync(string claimData, string? context = null)
    {
        try
        {
            // Build processing context
            var processingContext = await BuildProcessingContextAsync();
            var fullContext = $"{processingContext}\n\n{context ?? string.Empty}";

            // Create chat history
            var history = new ChatHistory();
            history.AddSystemMessage(GetSystemPrompt());

            var textContent = new TextContent($"Please process this medical claim for adjudication:\n\nClaim Data: {claimData}\n\nContext: {fullContext}");

            history.AddUserMessage([textContent]);


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
            var response = await _reviewChatService.GetChatMessageContentAsync(history, executionSettings);
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

    public async Task<ExtractedClaimData> ExtractClaimDataStructuredAsync(IFormFile file)
    {
        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var fileBytes = stream.ToArray();

            var fileContent = new BinaryContent(fileBytes, "application/pdf");

            var jsonSchema = """
                {
                    "type": "object",
                    "properties": {
                        "PatientFirstName": {
                            "type": "string",
                            "description": "Patient's first name"
                        },
                        "PatientSurname": {
                            "type": "string", 
                            "description": "Patient's surname/last name"
                        },

                        "TransactionNumber": {
                            "type": "string",
                            "description": "Transaction number if available, empty string if not found"
                        },
                        "MedicalSchemeName": {
                            "type": "string",
                            "description": "Medical scheme/insurance name, empty string if not found"
                        },
                        "TotalClaimAmount": {
                            "type": "number",
                            "description": "Total claim amount in decimal format, 0 if not found"
                        },
                        "ClaimMarkdown": {
                            "type": "string",
                            "description": "Full claim data extracted in markdown format with all available details"
                        }
                    },
                    "required": ["PatientFirstName", "PatientSurname", "TransactionNumber", "MedicalSchemeName", "TotalClaimAmount", "ClaimMarkdown"],
                    "additionalProperties": false
                }
                """;

            var chatResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "extracted_claim_data",
                jsonSchema: BinaryData.FromString(jsonSchema),
                jsonSchemaIsStrict: true
            );

            var history = new ChatHistory(@"Extract claim data from the uploaded medical claim form. 
                Extract patient information and present ALL available claim details in structured markdown format.
                
                IMPORTANT: You must provide ALL required fields:
                - PatientFirstName and PatientSurname: Extract from the form
                - TransactionNumber: Look for transaction/reference numbers, use empty string if not found
                - MedicalSchemeName: Look for medical scheme/insurance names, use empty string if not found  
                - TotalClaimAmount: Extract total amount claimed, use 0 if not found
                - ClaimMarkdown: Comprehensive, well-formatted representation of ALL visible claim data
                
                Extract what you can see clearly and use appropriate defaults for missing required fields.");
            
            history.AddUserMessage([fileContent]);

            var executionSettings = new AzureOpenAIPromptExecutionSettings
            {
                MaxTokens = 16000,
                ResponseFormat = chatResponseFormat,
            };

            var response = await _extractionChatService.GetChatMessageContentAsync(history, executionSettings);
            var responseContent = response.Content;

            if (!string.IsNullOrWhiteSpace(responseContent))
            {
                try
                {
                    var extractedData = JsonSerializer.Deserialize<ExtractedClaimData>(responseContent);
                    return extractedData ?? new ExtractedClaimData();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not parse extraction response as ExtractedClaimData. Raw response: {Response}", responseContent);
                }
            }

            return new ExtractedClaimData 
            { 
                ClaimMarkdown = "# Extraction Failed\n\nAn error occurred while extracting data from the uploaded PDF."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting structured claim data from uploaded PDF");
            return new ExtractedClaimData 
            { 
                ClaimMarkdown = "# Extraction Failed\n\nAn error occurred while extracting data from the uploaded PDF."
            };
        }
    }

    public async Task<ClaimModel> ProcessUploadedClaimAsync(IFormFile file)
    {
        try
        {
            _logger.LogInformation("Processing uploaded claim file: {FileName}", file.FileName);

            // Extract basic claim data
            var extractedData = await ExtractClaimDataStructuredAsync(file);

            // Create claim model with extracted data
            var claimModel = new ClaimModel
            {
                Id = Guid.NewGuid(),
                Source = ClaimSource.Upload,
                Status = ClaimStatus.UnderAgentReview,
                IngestedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                
                // Basic extracted fields
                PatientFirstName = extractedData.PatientFirstName,
                PatientSurname = extractedData.PatientSurname,
                ClaimNumber = !string.IsNullOrEmpty(extractedData.ClaimNumber)
                    ? extractedData.ClaimNumber
                    : $"CLM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                TransactionNumber = !string.IsNullOrEmpty(extractedData.TransactionNumber) 
                    ? extractedData.TransactionNumber 
                    : $"UPL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                MedicalSchemeName = extractedData.MedicalSchemeName,
                TotalClaimAmount = extractedData.TotalClaimAmount,
                ClaimMarkdown = extractedData.ClaimMarkdown,

                // Empty CIMAS data since this is an upload
                Data = new()
            };

            // Get agent review
            var agentReview = await TestAgentAsync(extractedData.ClaimMarkdown);
            if (agentReview != null)
            {
                claimModel.AgentReview = agentReview;
                claimModel.Status = ClaimStatus.UnderHumanReview;
            }

            _logger.LogInformation("Successfully processed uploaded claim {ClaimId}", claimModel.Id);
            return claimModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing uploaded claim");
            throw;
        }
    }
}

public class ExtractedClaimData
{
    public string PatientFirstName { get; set; } = string.Empty;
    public string PatientSurname { get; set; } = string.Empty;
    public string ClaimNumber { get; set; } = string.Empty;
    public string TransactionNumber { get; set; } = string.Empty;
    public string MedicalSchemeName { get; set; } = string.Empty;
    public decimal TotalClaimAmount { get; set; } = 0;
    public string ClaimMarkdown { get; set; } = string.Empty;
}
