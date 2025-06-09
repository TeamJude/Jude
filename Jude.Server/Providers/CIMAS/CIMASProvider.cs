using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jude.Server.Config;
using Jude.Server.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace Jude.Server.Providers.CIMAS;

public interface ICIMASProvider
{
    Task<Result<TokenPair>> GetAccessTokenAsync();
    Task<Result<Member>> GetMemberAsync(GetMemberInput input);
    Task<Result<List<Claim>>> GetPastClaimsAsync(GetPastClaimsInput input);
}

public class CIMASProvider : ICIMASProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CIMASProvider> _logger;
    private readonly CIMASConfig _config;

    public CIMASProvider(
        HttpClient httpClient,
        ILogger<CIMASProvider> logger,
        CIMASConfig config
    )
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
    }

    public async Task<Result<TokenPair>> GetAccessTokenAsync()
    {
        _logger.LogInformation("Getting access token");

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_config.ClaimsSwitchEndpoint}/login/",
                new { acc_name = _config.AccountName, password = _config.AccountPassword }
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get access token. Status: {Status}", response.StatusCode);
                return Result.Fail("Failed to get access token");
            }

            var data = await response.Content.ReadFromJsonAsync<APIResponse<TokenResponse>>();
            if (data == null)
            {
                _logger.LogError("Failed to deserialize token response");
                return Result.Fail("Failed to deserialize token response");
            }

            return Result.Ok(new TokenPair(data.Data.Tokens.Access, data.Data.Tokens.Refresh));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting access token");
            return Result.Exception("Error getting access token");
        }
    }

    public async Task<Result<Member>> GetMemberAsync(GetMemberInput input)
    {
        _logger.LogInformation("Getting member info for {MembershipNumber}", input.MembershipNumber);

        try
        {
            var response = await _httpClient.GetAsync(
                $"{_config.ClaimsSwitchEndpoint}/members/{input.MembershipNumber}/{input.Suffix}/"
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Failed to get member info. Status: {Status}",
                    response.StatusCode
                );
                return Result.Fail("Failed to get member info");
            }

            var data = await response.Content.ReadFromJsonAsync<APIResponse<Member>>();
            if (data == null)
            {
                _logger.LogError("Failed to deserialize member response");
                return Result.Fail("Failed to deserialize member response");
            }

            return Result.Ok(data.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting member info");
            return Result.Exception("Error getting member info");
        }
    }

    public async Task<Result<List<Claim>>> GetPastClaimsAsync(GetPastClaimsInput input)
    {
        _logger.LogInformation(
            "Getting past claims for practice {PracticeNumber}",
            input.PracticeNumber
        );

        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_config.ClaimsSwitchEndpoint}/claims/past/claims/{input.PracticeNumber}/"
            );
            request.Headers.Add("Authorization", $"Bearer {input.AccessToken}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Failed to get past claims. Status: {Status}",
                    response.StatusCode
                );
                return Result.Fail("Failed to get past claims");
            }

            var data = await response.Content.ReadFromJsonAsync<APIResponse<ClaimsResponse>>();
            if (data == null)
            {
                _logger.LogError("Failed to deserialize claims response");
                return Result.Fail("Failed to deserialize claims response");
            }

            return Result.Ok(data.Data.Results.Select(r => r.Response).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting past claims");
            return Result.Exception("Error getting past claims");
        }
    }

    private record TokenResponse
    {
        [JsonPropertyName("tokens")]
        public TokenInfo Tokens { get; set; } = new();
    }

    private record TokenInfo
    {
        [JsonPropertyName("access")]
        public string Access { get; set; } = string.Empty;

        [JsonPropertyName("refresh")]
        public string Refresh { get; set; } = string.Empty;
    }

    private record ClaimsResponse
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public object? Next { get; set; }

        [JsonPropertyName("previous")]
        public object? Previous { get; set; }

        [JsonPropertyName("results")]
        public List<ClaimWrapper> Results { get; set; } = [];
    }

    private record ClaimWrapper
    {
        [JsonPropertyName("Response")]
        public Claim Response { get; set; } = new();
    }
}