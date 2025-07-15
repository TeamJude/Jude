using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Claims.Providers.CIMAS;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Domains.Claims;

public interface IClaimsService
{
    Task<Result<Member>> GetMemberAsync(int membershipNumber, int suffix);
    Task<Result<List<ClaimResponse>>> GetPastClaimsAsync(string practiceNumber);
    Task<Result<ClaimResponse>> SubmitClaimAsync(ClaimRequest request);
    Task<Result<bool>> ReverseClaimAsync(string transactionNumber);
    Task<Result<bool>> UpdateClaimAsync(ClaimModel claim);
    Task<Result<GetClaimsResponse>> GetClaimsAsync(GetClaimsRequest request);
    Task<Result<ClaimDetailResponse>> GetClaimAsync(Guid claimId);

    // Dashboard Statistics methods
    Task<Result<ClaimStats>> GetClaimStatsAsync(GetClaimStatsInput input);
    Task<Result<MemberStats>> GetMemberStatsAsync(GetMemberStatsInput input);
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

    public async Task<Result<GetClaimsResponse>> GetClaimsAsync(GetClaimsRequest request)
    {
        try
        {
            var query = _repository.Claims.AsQueryable();            // Apply filters
            if (request.Status != null && request.Status.Length > 0)
            {
                query = query.Where(c => request.Status.Contains(c.Status));
            }
            if (request.RiskLevel != null && request.RiskLevel.Length > 0)
            {
                query = query.Where(c => request.RiskLevel.Contains(c.FraudRiskLevel));
            }

            if (request.RequiresHumanReview.HasValue)
            {
                query = query.Where(c => c.RequiresHumanReview == request.RequiresHumanReview.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchTerm = request.Search.ToLower();
                query = query.Where(c =>
                    c.PatientName.ToLower().Contains(searchTerm) ||
                    c.TransactionNumber.ToLower().Contains(searchTerm) ||
                    c.MembershipNumber.ToLower().Contains(searchTerm) ||
                    c.ProviderPractice.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var claims = await query
                .OrderByDescending(c => c.IngestedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new ClaimSummaryResponse(
                    c.Id,
                    c.TransactionNumber,
                    c.PatientName,
                    c.MembershipNumber,
                    c.ProviderPractice,
                    c.ClaimAmount,
                    c.ApprovedAmount,
                    c.Currency,
                    c.Status,
                    c.Source,
                    c.SubmittedAt ?? c.IngestedAt,
                    c.FraudRiskLevel,
                    c.IsFlagged,
                    c.RequiresHumanReview,
                    c.AgentRecommendation,
                    c.IngestedAt,
                    c.UpdatedAt
                ))
                .ToArrayAsync();

            return Result.Ok(new GetClaimsResponse(claims, totalCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving claims");
            return Result.Fail($"Failed to retrieve claims: {ex.Message}");
        }
    }

    public async Task<Result<ClaimDetailResponse>> GetClaimAsync(Guid claimId)
    {
        try
        {
            var claim = await _repository.Claims
                .Include(c => c.ReviewedBy)
                .FirstOrDefaultAsync(c => c.Id == claimId);

            if (claim == null)
            {
                return Result.Fail("Claim not found");
            }

            var reviewerInfo = claim.ReviewedBy != null
                ? new ReviewerInfo(claim.ReviewedBy.Id, claim.ReviewedBy.Username, claim.ReviewedBy.Email)
                : null;

            var response = new ClaimDetailResponse(
                claim.Id,
                claim.TransactionNumber,
                claim.ClaimNumber,
                claim.PatientName,
                claim.MembershipNumber,
                claim.ProviderPractice,
                claim.ClaimAmount,
                claim.ApprovedAmount,
                claim.Currency,
                claim.Status,
                claim.Source,
                claim.SubmittedAt,
                claim.ProcessedAt,
                claim.AgentRecommendation,
                claim.AgentReasoning,
                claim.AgentConfidenceScore,
                claim.AgentProcessedAt,
                claim.FraudIndicators,
                claim.FraudRiskLevel,
                claim.IsFlagged,
                claim.RequiresHumanReview,
                claim.FinalDecision,
                claim.ReviewerComments,
                claim.RejectionReason,
                claim.ReviewedAt,
                reviewerInfo,
                claim.IngestedAt,
                claim.UpdatedAt,
                claim.CIMASPayload
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving claim {ClaimId}", claimId);
            return Result.Fail($"Failed to retrieve claim: {ex.Message}");
        }
    }

    public async Task<Result<ClaimStats>> GetClaimStatsAsync(GetClaimStatsInput input)
    {
        var authResult = await EnsureAuthenticationAsync();
        if (!authResult.Success)
        {
            return Result.Fail(authResult.Errors);
        }

        var accessToken = _cache.Get<string>(ACCESS_TOKEN_KEY)!;
        var statsInput = new GetClaimStatsInput(accessToken, input.PracticeNumber, input.FromDate, input.ToDate);

        return await _cimasProvider.GetClaimStatsAsync(statsInput);
    }

    public async Task<Result<MemberStats>> GetMemberStatsAsync(GetMemberStatsInput input)
    {
        var authResult = await EnsureAuthenticationAsync();
        if (!authResult.Success)
        {
            return Result.Fail(authResult.Errors);
        }

        var accessToken = _cache.Get<string>(ACCESS_TOKEN_KEY)!;
        var statsInput = new GetMemberStatsInput(accessToken, input.PracticeNumber, input.FromDate, input.ToDate);

        return await _cimasProvider.GetMemberStatsAsync(statsInput);
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
