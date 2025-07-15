using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jude.Server.Config;
using Jude.Server.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace Jude.Server.Domains.Claims.Providers.CIMAS;

public interface ICIMASProvider
{
    Task<Result<TokenPair>> GetAccessTokenAsync();
    Task<Result<TokenPair>> RefreshAccessTokenAsync(string refreshToken);
    Task<Result<Member>> GetMemberAsync(GetMemberInput input);
    Task<Result<List<ClaimResponse>>> GetPastClaimsAsync(GetPastClaimsInput input);
    Task<Result<ClaimResponse>> SubmitClaimAsync(SubmitClaimInput input);
    Task<Result<bool>> ReverseClaimAsync(ReverseClaimInput input);
    Task<Result<bool>> UploadDocumentAsync(UploadDocumentInput input);

    // Pricing API methods
    Task<Result<string>> GetPricingAccessTokenAsync();
    Task<Result<TariffResponse>> GetTariffByCodeAsync(TariffLookupInput input);
}

public class CIMASProvider : ICIMASProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CIMASProvider> _logger;

    public CIMASProvider(HttpClient httpClient, ILogger<CIMASProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<TokenPair>> GetAccessTokenAsync()
    {
        _logger.LogInformation("Getting access token");

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{AppConfig.CIMAS.ClaimsSwitchEndpoint}/login/",
                new
                {
                    acc_name = AppConfig.CIMAS.AccountName,
                    password = AppConfig.CIMAS.AccountPassword,
                }
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Failed to get access token. Status: {Status}",
                    response.StatusCode
                );
                return Result.Fail("Failed to get access token");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("CIMAS login response: {Response}", responseContent);

            var apiResponse = await JsonSerializer.DeserializeAsync<APIResponse<TokenResponse>>(
                new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseContent))
            );

            if (apiResponse == null)
            {
                _logger.LogError("Failed to deserialize token response");
                return Result.Fail("Failed to deserialize token response");
            }

            if (apiResponse.Status != 200)
            {
                _logger.LogError(
                    "API returned error status: {Status}, Message: {Message}",
                    apiResponse.Status,
                    apiResponse.Message
                );
                return Result.Fail(apiResponse.Message);
            }

            _logger.LogDebug(
                "Parsed tokens - Access: {HasAccess}, Refresh: {HasRefresh}",
                !string.IsNullOrEmpty(apiResponse.Data.Tokens.Access),
                !string.IsNullOrEmpty(apiResponse.Data.Tokens.Refresh)
            );

            var tokenPair = new TokenPair(
                apiResponse.Data.Tokens.Access,
                apiResponse.Data.Tokens.Refresh ?? string.Empty
            );
            return Result.Ok(tokenPair);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting access token");
            return Result.Exception("Error getting access token");
        }
    }

    public async Task<Result<TokenPair>> RefreshAccessTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Refreshing access token");

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{AppConfig.CIMAS.ClaimsSwitchEndpoint}/jwt/refresh/",
                new { refresh = refreshToken }
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to refresh token. Status: {Status}", response.StatusCode);
                return Result.Fail("Failed to refresh token");
            }

            var data = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
            if (data == null)
            {
                _logger.LogError("Failed to deserialize refresh token response");
                return Result.Fail("Failed to deserialize refresh token response");
            }

            return Result.Ok(new TokenPair(data.Access, refreshToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing access token");
            return Result.Exception("Error refreshing access token");
        }
    }

    public async Task<Result<Member>> GetMemberAsync(GetMemberInput input)
    {
        _logger.LogInformation(
            "Getting member info for {MembershipNumber}",
            input.MembershipNumber
        );

        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{AppConfig.CIMAS.ClaimsSwitchEndpoint}/claims/2cana/member/details/{input.MembershipNumber}-{input.Suffix}/cimas/"
            );
            request.Headers.Add("Authorization", $"Bearer {input.AccessToken}");

            var response = await _httpClient.SendAsync(request);

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

    public async Task<Result<List<ClaimResponse>>> GetPastClaimsAsync(GetPastClaimsInput input)
    {
        _logger.LogInformation(
            "Getting past claims for practice {PracticeNumber}",
            input.PracticeNumber
        );

        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{AppConfig.CIMAS.ClaimsSwitchEndpoint}/claims/past/claims/{input.PracticeNumber}/"
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

            var data = await response.Content.ReadFromJsonAsync<ClaimsResponse>();
            _logger.LogDebug("CIMAS past claims response: {Response}", data);

            if (data == null)
            {
                _logger.LogError("Failed to deserialize claims response");
                return Result.Fail("Failed to deserialize claims response");
            }

            _logger.LogDebug("Parsed {Count} claims from CIMAS response", data.Results.Count);

            return Result.Ok(data.Results.Select(r => r.Response).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting past claims");
            return Result.Exception("Error getting past claims");
        }
    }

    public async Task<Result<ClaimResponse>> SubmitClaimAsync(SubmitClaimInput input)
    {
        _logger.LogInformation(
            "Submitting claim for member {MemberNumber}",
            input.Request.Member.MedicalSchemeNumber
        );

        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{AppConfig.CIMAS.ClaimsSwitchEndpoint}/claims/create/fullclaim/"
            );
            request.Headers.Add("Authorization", $"Bearer {input.AccessToken}");
            request.Content = JsonContent.Create(new { Request = input.Request });

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to submit claim. Status: {Status}", response.StatusCode);
                return Result.Fail("Failed to submit claim");
            }

            var data = await response.Content.ReadFromJsonAsync<APIResponse<ClaimResponse>>();
            if (data == null)
            {
                _logger.LogError("Failed to deserialize claim response");
                return Result.Fail("Failed to deserialize claim response");
            }

            return Result.Ok(data.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting claim");
            return Result.Exception("Error submitting claim");
        }
    }

    public async Task<Result<bool>> ReverseClaimAsync(ReverseClaimInput input)
    {
        _logger.LogInformation("Reversing claim {TransactionNumber}", input.TransactionNumber);

        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{AppConfig.CIMAS.ClaimsSwitchEndpoint}/claims/reverse"
            );
            request.Headers.Add("Authorization", $"Bearer {input.AccessToken}");
            request.Content = JsonContent.Create(
                new { transaction_number = input.TransactionNumber }
            );

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to reverse claim. Status: {Status}", response.StatusCode);
                return Result.Fail("Failed to reverse claim");
            }

            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reversing claim");
            return Result.Exception("Error reversing claim");
        }
    }

    public async Task<Result<bool>> UploadDocumentAsync(UploadDocumentInput input)
    {
        _logger.LogInformation(
            "Uploading document for claim {TransactionNumber}",
            input.TransactionNumber
        );

        try
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(input.FileStream), "file", input.FileName);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{AppConfig.CIMAS.ClaimsSwitchEndpoint}/claims/upload-file/{input.TransactionNumber}/{input.Channel}"
            );
            request.Headers.Add("Authorization", $"Bearer {input.AccessToken}");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Failed to upload document. Status: {Status}",
                    response.StatusCode
                );
                return Result.Fail("Failed to upload document");
            }

            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return Result.Exception("Error uploading document");
        }
    }

    public async Task<Result<string>> GetPricingAccessTokenAsync()
    {
        _logger.LogInformation("Getting pricing API access token");

        try
        {
            var requestBody = new PricingApiTokenRequest
            {
                Username = AppConfig.CIMAS.PricingApiUsername,
                Password = AppConfig.CIMAS.PricingApiPassword
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{AppConfig.CIMAS.PricingApiEndpoint}/api/v1/auth/login",
                requestBody
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Failed to get pricing API access token. Status: {Status}",
                    response.StatusCode
                );
                return Result.Fail("Failed to get pricing API access token");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Pricing API login response: {Response}", responseContent);

            var tokenResponse = await response.Content.ReadFromJsonAsync<PricingApiTokenResponse>();
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                _logger.LogError("Failed to deserialize pricing API token response or token is empty");
                return Result.Fail("Failed to deserialize pricing API token response");
            }

            _logger.LogInformation("Successfully obtained pricing API access token");
            return Result.Ok(tokenResponse.AccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pricing API access token");
            return Result.Exception("Error getting pricing API access token");
        }
    }

    public async Task<Result<TariffResponse>> GetTariffByCodeAsync(TariffLookupInput input)
    {
        _logger.LogInformation("Looking up tariff for code {TariffCode}", input.TariffCode);

        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{AppConfig.CIMAS.PricingApiEndpoint}/api/v1/tariff/by/code/{input.TariffCode}"
            );
            request.Headers.Add("Authorization", $"Bearer {input.PricingAccessToken}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                // Check if it's a 400 error with specific error message
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Pricing API error response: {ErrorContent}", errorContent);

                    try
                    {
                        var errorResponse = await JsonSerializer.DeserializeAsync<PricingApiErrorResponse>(
                            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(errorContent))
                        );

                        if (errorResponse != null)
                        {
                            _logger.LogWarning(
                                "Tariff lookup failed: {ErrorMsg} - {DeveloperMsg}",
                                errorResponse.ErrorMsg,
                                errorResponse.DeveloperMsg
                            );
                            return Result.Fail($"Tariff not found: {errorResponse.DeveloperMsg}");
                        }
                    }
                    catch (JsonException)
                    {
                        // If we can't parse the error, fall through to generic error handling
                    }
                }

                _logger.LogError(
                    "Failed to get tariff info. Status: {Status}",
                    response.StatusCode
                );
                return Result.Fail("Failed to get tariff info");
            }

            var tariffResponse = await response.Content.ReadFromJsonAsync<TariffResponse>();
            if (tariffResponse == null)
            {
                _logger.LogError("Failed to deserialize tariff response");
                return Result.Fail("Failed to deserialize tariff response");
            }

            _logger.LogInformation(
                "Successfully retrieved tariff {TariffCode}: {Description}",
                tariffResponse.Code,
                tariffResponse.Description
            );

            return Result.Ok(tariffResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tariff info for code {TariffCode}", input.TariffCode);
            return Result.Exception("Error getting tariff info");
        }
    }
}
