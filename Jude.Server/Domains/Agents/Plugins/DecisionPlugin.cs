using System.ComponentModel;
using System.Text.Json;
using Jude.Server.Data.Models;
using Jude.Server.Domains.Claims;
using Microsoft.SemanticKernel;

namespace Jude.Server.Domains.Agents.Plugins;

public class DecisionPlugin
{
    private readonly ClaimModel _claim;
    private readonly IClaimsService _claimsService;
    private readonly ILogger<DecisionPlugin> _logger;

    public DecisionPlugin(
        ClaimModel claim,
        IClaimsService claimsService,
        ILogger<DecisionPlugin> logger
    )
    {
        _claim = claim;
        _claimsService = claimsService;
        _logger = logger;
    }

    [KernelFunction]
    [Description(
        "Make a decision for the claim by updating the recommendation, reasoning, confidence score, and other relevant fields"
    )]
    public async Task<string> MakeDecision(
        [Description(
            "The recommendation for the claim (APPROVE, DENY, PENDING, REVIEW, INVESTIGATE)"
        )]
            string recommendation,
        [Description("List of reasoning steps the you went through during processing")]
            List<string>? reasoningLog = null,
        [Description("Confidence score between 0.0 and 1.0")] decimal confidenceScore = 0.0m,
        [Description("Fraud risk level (Low, Medium, High, Critical)")]
            string fraudRiskLevel = "Low",
        [Description("Whether the claim requires human review (true/false)")]
            bool requiresHumanReview = false,
        [Description("Approved amount if different from claimed amount")]
            decimal? approvedAmount = null,
        [Description(
            "Policy citations - pipe-separated list of policy sources that support this decision (e.g., 'Section 3.15|Section 2.12|Coverage Policy 4.1')"
        )]
            string? policyCitations = null,
        [Description(
            "Policy quotes - pipe-separated list of exact quotes from policies (must match order of policyCitations)"
        )]
            string? policyQuotes = null,
        [Description(
            "Tariff citations - pipe-separated list of tariff codes used in decision (e.g., '98101|98411|CON-2023')"
        )]
            string? tariffCitations = null,
        [Description(
            "Tariff details - pipe-separated list of tariff descriptions and pricing info (must match order of tariffCitations)"
        )]
            string? tariffDetails = null,
        [Description(
            "Citation contexts - pipe-separated list explaining how each citation supports the decision (must match total citation count)"
        )]
            string? citationContexts = null
    )
    {
        try
        {
            _logger.LogInformation(
                "Making decision for claim {ClaimId}: {Recommendation}",
                _claim.Id,
                recommendation
            );

            // Update claim with agent decision
            _claim.AgentRecommendation = recommendation.ToUpper();
            _claim.AgentReasoningLog = reasoningLog ?? [];
            _claim.AgentConfidenceScore = Math.Max(0.0m, Math.Min(1.0m, confidenceScore)); // Clamp between 0 and 1
            _claim.RequiresHumanReview = requiresHumanReview;
            _claim.AgentProcessedAt = DateTime.UtcNow;

            // Set approved amount if provided, or full amount for approvals
            if (approvedAmount.HasValue)
            {
                _claim.ApprovedAmount = approvedAmount.Value;
            }
            else if (recommendation.ToUpper() == "APPROVE")
            {
                _claim.ApprovedAmount = _claim.ClaimAmount;
            }

            // Parse and set fraud risk level
            if (Enum.TryParse<FraudRiskLevel>(fraudRiskLevel, true, out var riskLevel))
            {
                _claim.FraudRiskLevel = riskLevel;
            }

            var citationList = new List<CitationModel>();

            if (!string.IsNullOrWhiteSpace(policyCitations))
            {
                var sources = policyCitations.Split('|', StringSplitOptions.RemoveEmptyEntries);
                var quotes = policyQuotes?.Split('|', StringSplitOptions.RemoveEmptyEntries) ?? [];
                var contexts =
                    citationContexts?.Split('|', StringSplitOptions.RemoveEmptyEntries) ?? [];

                for (int i = 0; i < sources.Length; i++)
                {
                    citationList.Add(
                        new CitationModel
                        {
                            Id = Guid.NewGuid(),
                            Type = "Policy",
                            Source = sources[i].Trim(),
                            Quote = i < quotes.Length ? quotes[i].Trim() : "",
                            Context = i < contexts.Length ? contexts[i].Trim() : "",
                            ClaimId = _claim.Id,
                            CitedAt = DateTime.UtcNow,
                        }
                    );
                }
            }

            // Process tariff citations
            if (!string.IsNullOrWhiteSpace(tariffCitations))
            {
                var sources = tariffCitations.Split('|', StringSplitOptions.RemoveEmptyEntries);
                var details =
                    tariffDetails?.Split('|', StringSplitOptions.RemoveEmptyEntries) ?? [];
                var contexts =
                    citationContexts?.Split('|', StringSplitOptions.RemoveEmptyEntries) ?? [];
                var contextOffset = citationList.Count; // Offset for tariff contexts

                for (int i = 0; i < sources.Length; i++)
                {
                    citationList.Add(
                        new CitationModel
                        {
                            Id = Guid.NewGuid(),
                            Type = "Tariff",
                            Source = sources[i].Trim(),
                            Quote = i < details.Length ? details[i].Trim() : "",
                            Context =
                                (contextOffset + i) < contexts.Length
                                    ? contexts[contextOffset + i].Trim()
                                    : "",
                            ClaimId = _claim.Id,
                            CitedAt = DateTime.UtcNow,
                        }
                    );
                }
            }

            if (citationList.Any())
            {
                _claim.Citations = citationList;
                _logger.LogInformation(
                    "Successfully parsed {CitationCount} citations for claim {ClaimId}",
                    citationList.Count,
                    _claim.Id
                );
            }

            // Update claim status based on recommendation
            _claim.Status = recommendation.ToUpper() switch
            {
                "APPROVE" => requiresHumanReview ? ClaimStatus.Review : ClaimStatus.Completed,
                "DENY" => ClaimStatus.Review, // Denials always require human review
                "PENDING" => ClaimStatus.Pending,
                "REVIEW" => ClaimStatus.Review,
                "INVESTIGATE" => ClaimStatus.Review,
                _ => ClaimStatus.Review,
            };

            if (
                _claim.Status == ClaimStatus.Completed
                && recommendation.Equals("APPROVE", StringComparison.CurrentCultureIgnoreCase)
            )
            {
                _claim.FinalDecision = ClaimDecision.Approved;
            }

            _claim.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _claimsService.UpdateClaimAsync(_claim);
            if (!updateResult.Success)
            {
                _logger.LogError(
                    "Failed to update claim {ClaimId}: {Error}",
                    _claim.Id,
                    updateResult.Errors.FirstOrDefault()
                );
                return $"Error updating claim: {updateResult.Errors.FirstOrDefault()}";
            }

            var result =
                $"Decision recorded successfully for claim {_claim.Id}:\n"
                + $"- Recommendation: {recommendation}\n"
                + $"- Confidence: {confidenceScore:P}\n"
                + $"- Status: {_claim.Status}\n"
                + $"- Requires Review: {requiresHumanReview}";

            if (approvedAmount.HasValue)
            {
                result += $"\n- Approved Amount: {approvedAmount:C}";
            }

            if (_claim.Citations?.Any() == true)
            {
                result += $"\n- Citations Used: {_claim.Citations.Count}";
            }

            _logger.LogInformation("Successfully recorded decision for claim {ClaimId}", _claim.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making decision for claim {ClaimId}", _claim.Id);
            return $"Error recording decision: {ex.Message}";
        }
    }
}
