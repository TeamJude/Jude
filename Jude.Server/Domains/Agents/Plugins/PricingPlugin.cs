 using System.ComponentModel;
using System.Text.Json;
using Jude.Server.Domains.Claims;
using Microsoft.SemanticKernel;

namespace Jude.Server.Domains.Agents.Plugins;

public class PricingPlugin
{
    private readonly IClaimsService _claimsService;
    private readonly ILogger<PricingPlugin> _logger;

    public PricingPlugin(IClaimsService claimsService, ILogger<PricingPlugin> logger)
    {
        _claimsService = claimsService;
        _logger = logger;
    }

    [KernelFunction]
    [Description(
        "Look up tariff pricing information for a specific medical procedure or service code. Use this to validate claim amounts against standard tariff rates and detect potential overcharging or billing irregularities."
    )]
    public async Task<string> GetTariffPricing(
        [Description(
            "The tariff code for the medical procedure or service. This is typically found in the claim's service response data or billing codes."
        )]
            string tariffCode
    )
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tariffCode))
            {
                _logger.LogWarning("Empty or null tariff code provided");
                return "Error: Tariff code is required. Please provide a valid medical procedure or service code.";
            }

            _logger.LogInformation("Looking up pricing for tariff code: {TariffCode}", tariffCode);

            var result = await _claimsService.GetTariffByCodeAsync(tariffCode);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to retrieve tariff pricing for code {TariffCode}: {Errors}", 
                    tariffCode, string.Join(", ", result.Errors));
                
                if (result.Errors.Any(e => e.Contains("not found")))
                {
                    return $"Tariff code '{tariffCode}' not found in pricing database. This may indicate an invalid or outdated procedure code that requires manual review.";
                }
                
                return $"Unable to retrieve pricing information for tariff code '{tariffCode}'. Error: {string.Join(", ", result.Errors)}";
            }



            _logger.LogInformation("Successfully retrieved pricing for tariff code: {TariffCode}", tariffCode);
            var responseJson = JsonSerializer.Serialize(result.Data);
            return responseJson;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tariff pricing for code: {TariffCode}", tariffCode);
            return $"Error retrieving pricing information for tariff code '{tariffCode}': {ex.Message}. Please try again or contact support.";
        }
    }
}