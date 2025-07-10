using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Claims.Providers.CIMAS;
using Microsoft.Extensions.Caching.Memory;

namespace Jude.Server.Domains.Claims;

public interface IClaimsService
{
    Task<Result<Member>> GetMemberAsync(int membershipNumber, int suffix);
    Task<Result<List<ClaimResponse>>> GetPastClaimsAsync(string practiceNumber);
    Task<Result<ClaimResponse>> SubmitClaimAsync(ClaimRequest request);
    Task<Result<bool>> ReverseClaimAsync(string transactionNumber);
    Task<Result<bool>> UpdateClaimAsync(ClaimModel claim);
}

public class ClaimsService : IClaimsService
{
    private readonly JudeDbContext _repository;
    private readonly ICIMASProvider _cimasProvider;
    private readonly ILogger<ClaimsService> _logger;
    private readonly IMemoryCache _cache;

    private const string ACCESS_TOKEN_KEY = "cimas_access_token";
    private const string REFRESH_TOKEN_KEY = "cimas_refresh_token";
    private static readonly TimeSpan TokenCacheExpiry = TimeSpan.FromMinutes(50); // Tokens usually expire in 60 minutes

    public ClaimsService(
        JudeDbContext repository,
        ICIMASProvider cimasProvider,
        ILogger<ClaimsService> logger,
        IMemoryCache cache
    )
    {
        _repository = repository;
        _cimasProvider = cimasProvider;
        _logger = logger;
        _cache = cache;
    }

    private async Task<Result<bool>> EnsureAuthenticationAsync()
    {
        var accessToken = _cache.Get<string>(ACCESS_TOKEN_KEY);

        if (!string.IsNullOrEmpty(accessToken))
        {
            _logger.LogDebug("Using cached access token");
            return Result.Ok(true);
        }

        var refreshToken = _cache.Get<string>(REFRESH_TOKEN_KEY);
        if (!string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogInformation("Attempting to refresh access token");
            var refreshResult = await _cimasProvider.RefreshAccessTokenAsync(refreshToken);

            if (refreshResult.Success && !string.IsNullOrEmpty(refreshResult.Data.AccessToken))
            {
                CacheTokens(refreshResult.Data);
                _logger.LogInformation("Successfully refreshed access token");
                return Result.Ok(true);
            }

            _logger.LogWarning(
                "Token refresh failed: {Errors}",
                string.Join(", ", refreshResult.Errors)
            );
        }

        _logger.LogInformation("Getting new access token from CIMAS");
        var tokenResult = await _cimasProvider.GetAccessTokenAsync();

        if (!tokenResult.Success)
        {
            _logger.LogError(
                "Failed to get access token: {Errors}",
                string.Join(", ", tokenResult.Errors)
            );
            return Result.Fail("Failed to authenticate with CIMAS");
        }

        if (string.IsNullOrEmpty(tokenResult.Data.AccessToken))
        {
            _logger.LogError("Received empty access token from CIMAS");
            return Result.Fail("Received invalid token from CIMAS");
        }

        CacheTokens(tokenResult.Data);
        _logger.LogInformation("Successfully obtained new access token");
        return Result.Ok(true);
    }

    public async Task<Result<Member>> GetMemberAsync(int membershipNumber, int suffix)
    {
        var authResult = await EnsureAuthenticationAsync();
        if (!authResult.Success)
        {
            return Result.Fail(authResult.Errors);
        }

        var accessToken = _cache.Get<string>(ACCESS_TOKEN_KEY)!;
        var input = new GetMemberInput(membershipNumber, suffix, accessToken);

        return await _cimasProvider.GetMemberAsync(input);
    }

    public async Task<Result<List<ClaimResponse>>> GetPastClaimsAsync(string practiceNumber)
    {
        var authResult = await EnsureAuthenticationAsync();
        if (!authResult.Success)
        {
            return Result.Fail(authResult.Errors);
        }

        var accessToken = _cache.Get<string>(ACCESS_TOKEN_KEY)!;
        var input = new GetPastClaimsInput(practiceNumber, accessToken);

        return await _cimasProvider.GetPastClaimsAsync(input);
    }

    public async Task<Result<ClaimResponse>> SubmitClaimAsync(ClaimRequest request)
    {
        var authResult = await EnsureAuthenticationAsync();
        if (!authResult.Success)
        {
            return Result.Fail(authResult.Errors);
        }

        var accessToken = _cache.Get<string>(ACCESS_TOKEN_KEY)!;
        var input = new SubmitClaimInput(request, accessToken);

        return await _cimasProvider.SubmitClaimAsync(input);
    }

    public async Task<Result<bool>> ReverseClaimAsync(string transactionNumber)
    {
        var authResult = await EnsureAuthenticationAsync();
        if (!authResult.Success)
        {
            return Result.Fail(authResult.Errors);
        }

        var accessToken = _cache.Get<string>(ACCESS_TOKEN_KEY)!;
        var input = new ReverseClaimInput(transactionNumber, accessToken);

        return await _cimasProvider.ReverseClaimAsync(input);
    }

    public async Task<Result<bool>> UpdateClaimAsync(ClaimModel claim)
    {
        try
        {
            claim.UpdatedAt = DateTime.UtcNow;
            _repository.Claims.Update(claim);
            await _repository.SaveChangesAsync();
            
            _logger.LogDebug("Successfully updated claim {ClaimId}", claim.Id);
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating claim {ClaimId}", claim.Id);
            return Result.Fail($"Failed to update claim: {ex.Message}");
        }
    }

    private void CacheTokens(TokenPair tokens)
    {
        if (string.IsNullOrEmpty(tokens.AccessToken))
        {
            _logger.LogWarning("Attempted to cache empty access token");
            return;
        }

        _cache.Set(ACCESS_TOKEN_KEY, tokens.AccessToken, TokenCacheExpiry);

        // Only cache refresh token if it's not empty
        if (!string.IsNullOrEmpty(tokens.RefreshToken))
        {
            _cache.Set(REFRESH_TOKEN_KEY, tokens.RefreshToken, TimeSpan.FromDays(7));
            _logger.LogDebug("Cached access token and refresh token");
        }
        else
        {
            _logger.LogDebug("Cached access token only (no refresh token provided)");
        }
    }
}
