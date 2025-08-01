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
    Task<Result<ClaimsDashboardResponse>> GetDashboardStatsAsync(ClaimsDashboardRequest request);
    Task<Result<TariffResponse>> GetTariffByCodeAsync(string tariffCode);
}

public class ClaimsService : IClaimsService
{
    private readonly JudeDbContext _repository;
    private readonly ICIMASProvider _cimasProvider;
    private readonly ILogger<ClaimsService> _logger;
    private readonly IMemoryCache _cache;

    private const string ACCESS_TOKEN_KEY = "cimas_access_token";
    private const string REFRESH_TOKEN_KEY = "cimas_refresh_token";
    private const string PRICING_TOKEN_KEY = "cimas_pricing_token";
    private static readonly TimeSpan TokenCacheExpiry = TimeSpan.FromMinutes(50); // Tokens usually expire in 60 minutes
    private static readonly TimeSpan PricingTokenCacheExpiry = TimeSpan.FromMinutes(50); // Pricing tokens usually expire in 60 minutes

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
    private double CalculateChangePercent(double current, double previous)
    {
        if (previous == 0)
        {
            return current > 0 ? 100.0 : 0.0;
        }
        return ((current - previous) / previous) * 100;
    }

    public async Task<Result<ClaimsDashboardResponse>> GetDashboardStatsAsync(ClaimsDashboardRequest request)
    {
        try
        {
            var now = DateTime.UtcNow;
            DateTime currentPeriodStart, previousPeriodStart, previousPeriodEnd;

            switch (request.Period)
            {
                case ClaimsDashboardPeriod.Last24Hours:
                    currentPeriodStart = now.Date;
                    previousPeriodEnd = currentPeriodStart;
                    previousPeriodStart = previousPeriodEnd.AddDays(-1);
                    break;
                case ClaimsDashboardPeriod.Last7Days:
                    currentPeriodStart = now.Date.AddDays(-6);
                    previousPeriodEnd = currentPeriodStart;
                    previousPeriodStart = previousPeriodEnd.AddDays(-7);
                    break;
                case ClaimsDashboardPeriod.Last30Days:
                    currentPeriodStart = now.Date.AddDays(-29);
                    previousPeriodEnd = currentPeriodStart;
                    previousPeriodStart = previousPeriodEnd.AddDays(-30);
                    break;
                case ClaimsDashboardPeriod.LastQuarter:
                    currentPeriodStart = now.Date.AddMonths(-3);
                    previousPeriodEnd = currentPeriodStart;
                    previousPeriodStart = previousPeriodEnd.AddMonths(-3);
                    break;
                default:
                    currentPeriodStart = now.Date.AddDays(-6);
                    previousPeriodEnd = currentPeriodStart;
                    previousPeriodStart = previousPeriodEnd.AddDays(-7);
                    break;
            }

            var currentPeriodQuery = _repository.Claims.Where(c => c.IngestedAt >= currentPeriodStart && c.IngestedAt <= now);
            var previousPeriodQuery = _repository.Claims.Where(c => c.IngestedAt >= previousPeriodStart && c.IngestedAt < previousPeriodEnd);

            // --- Current Period Stats ---
            int totalClaims = await currentPeriodQuery.CountAsync();
            int autoApproved = await currentPeriodQuery.CountAsync(c => c.FinalDecision == ClaimDecision.Approved && c.AgentRecommendation == "Auto-Approved");
            var processingTimes = await currentPeriodQuery
                .Where(c => c.AgentProcessedAt.HasValue)
                .Select(c => new { c.IngestedAt, c.AgentProcessedAt })
                .ToListAsync();
            int claimsFlagged = await currentPeriodQuery.CountAsync(c => c.IsFlagged);

            // --- Previous Period Stats ---
            int prevTotalClaims = await previousPeriodQuery.CountAsync();
            int prevAutoApproved = await previousPeriodQuery.CountAsync(c => c.FinalDecision == ClaimDecision.Approved && c.AgentRecommendation == "Auto-Approved");
            var prevProcessingTimes = await previousPeriodQuery
                .Where(c => c.AgentProcessedAt.HasValue)
                .Select(c => new { c.IngestedAt, c.AgentProcessedAt })
                .ToListAsync();
            int prevClaimsFlagged = await previousPeriodQuery.CountAsync(c => c.IsFlagged);

            // --- Calculate Final Stats ---
            double avgProcessingTime = processingTimes.Any()
                ? processingTimes.Average(c => (c.AgentProcessedAt.Value - c.IngestedAt).TotalMinutes)
                : 0;
            double prevAvgProcessingTime = prevProcessingTimes.Any()
                ? prevProcessingTimes.Average(c => (c.AgentProcessedAt.Value - c.IngestedAt).TotalMinutes)
                : 0;

            double autoApprovedRate = totalClaims > 0 ? (double)autoApproved / totalClaims * 100 : 0;
            double prevAutoApprovedRate = prevTotalClaims > 0 ? (double)prevAutoApproved / prevTotalClaims * 100 : 0;

            // --- Calculate Change Percentages ---
            double totalClaimsChangePercent = CalculateChangePercent(totalClaims, prevTotalClaims);
            double autoApprovedRateChangePercent = CalculateChangePercent(autoApprovedRate, prevAutoApprovedRate);
            double avgProcessingTimeChangePercent = CalculateChangePercent(avgProcessingTime, prevAvgProcessingTime);
            double claimsFlaggedChangePercent = CalculateChangePercent(claimsFlagged, prevClaimsFlagged);

            // --- Activity Chart Data ---
            var activity = await GetClaimsActivityAsync(currentPeriodQuery, request.Period, currentPeriodStart);

            var response = new ClaimsDashboardResponse(
                totalClaims,
                autoApprovedRate,
                avgProcessingTime,
                claimsFlagged,
                totalClaimsChangePercent,
                autoApprovedRateChangePercent,
                avgProcessingTimeChangePercent,
                claimsFlaggedChangePercent,
                activity
            );
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dashboard stats");
            return Result.Fail($"Failed to generate dashboard stats: {ex.Message}");
        }
    }

    private async Task<List<ClaimsActivityResponse>> GetClaimsActivityAsync(IQueryable<ClaimModel> query, ClaimsDashboardPeriod period, DateTime startDate)
    {
        var activity = new List<ClaimsActivityResponse>();

        switch (period)
        {
            case ClaimsDashboardPeriod.Last7Days:
                var dailyData = await query
                    .GroupBy(c => c.IngestedAt.Date)
                    .Select(g => new { Date = g.Key, Total = g.Count(), Processed = g.Count(c => c.Status != ClaimStatus.Pending), Approved = g.Count(c => c.FinalDecision == ClaimDecision.Approved), Rejected = g.Count(c => c.FinalDecision == ClaimDecision.Rejected) })
                    .ToDictionaryAsync(x => x.Date, x => x);

                for (int i = 0; i < 7; i++)
                {
                    var date = startDate.AddDays(i).Date;
                    if (dailyData.TryGetValue(date, out var data))
                    {
                        activity.Add(new ClaimsActivityResponse(date.ToString("ddd"), data.Total, data.Processed, data.Approved, data.Rejected));
                    }
                    else
                    {
                        activity.Add(new ClaimsActivityResponse(date.ToString("ddd"), 0, 0, 0, 0));
                    }
                }
                break;

            case ClaimsDashboardPeriod.Last30Days:
                var monthlyData = await query
                    .GroupBy(c => c.IngestedAt.Date)
                    .Select(g => new { Date = g.Key, Total = g.Count(), Processed = g.Count(c => c.Status != ClaimStatus.Pending), Approved = g.Count(c => c.FinalDecision == ClaimDecision.Approved), Rejected = g.Count(c => c.FinalDecision == ClaimDecision.Rejected) })
                    .ToDictionaryAsync(x => x.Date, x => x);

                for (int i = 0; i < 30; i++)
                {
                    var date = startDate.AddDays(i).Date;
                    if (monthlyData.TryGetValue(date, out var data))
                    {
                        activity.Add(new ClaimsActivityResponse(date.ToString("MM-dd"), data.Total, data.Processed, data.Approved, data.Rejected));
                    }
                    else
                    {
                        activity.Add(new ClaimsActivityResponse(date.ToString("MM-dd"), 0, 0, 0, 0));
                    }
                }
                break;

            case ClaimsDashboardPeriod.LastQuarter:
                var quarterlyData = await query
                    .GroupBy(c => new { c.IngestedAt.Year, c.IngestedAt.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Count(), Processed = g.Count(c => c.Status != ClaimStatus.Pending), Approved = g.Count(c => c.FinalDecision == ClaimDecision.Approved), Rejected = g.Count(c => c.FinalDecision == ClaimDecision.Rejected) })
                    .ToDictionaryAsync(x => new { x.Year, x.Month }, x => x);

                for (int i = 0; i < 4; i++)
                {
                    var date = startDate.AddMonths(i);
                    if (quarterlyData.TryGetValue(new { date.Year, date.Month }, out var data))
                    {
                        activity.Add(new ClaimsActivityResponse(date.ToString("MMM yyyy"), data.Total, data.Processed, data.Approved, data.Rejected));
                    }
                    else
                    {
                        activity.Add(new ClaimsActivityResponse(date.ToString("MMM yyyy"), 0, 0, 0, 0));
                    }
                }
                break;

            case ClaimsDashboardPeriod.Last24Hours:
                var hourlyData = await query
                    .Where(c => c.IngestedAt.Date == DateTime.UtcNow.Date)
                    .GroupBy(c => c.IngestedAt.Hour)
                    .Select(g => new { Hour = g.Key, Total = g.Count(), Processed = g.Count(c => c.Status != ClaimStatus.Pending), Approved = g.Count(c => c.FinalDecision == ClaimDecision.Approved), Rejected = g.Count(c => c.FinalDecision == ClaimDecision.Rejected) })
                    .ToDictionaryAsync(x => x.Hour, x => x);

                for (int i = 0; i < 24; i++)
                {
                    if (hourlyData.TryGetValue(i, out var data))
                    {
                        activity.Add(new ClaimsActivityResponse(i.ToString("D2") + ":00", data.Total, data.Processed, data.Approved, data.Rejected));
                    }
                    else
                    {
                        activity.Add(new ClaimsActivityResponse(i.ToString("D2") + ":00", 0, 0, 0, 0));
                    }
                }
                break;
        }
        return activity;
    }

    private async Task<Result<bool>> EnsurePricingAuthenticationAsync()
    {
        var pricingToken = _cache.Get<string>(PRICING_TOKEN_KEY);

        if (!string.IsNullOrEmpty(pricingToken))
        {
            _logger.LogDebug("Using cached pricing access token");
            return Result.Ok(true);
        }

        _logger.LogInformation("Getting new pricing access token from CIMAS");
        var tokenResult = await _cimasProvider.GetPricingAccessTokenAsync();

        if (!tokenResult.Success)
        {
            _logger.LogError(
                "Failed to get pricing access token: {Errors}",
                string.Join(", ", tokenResult.Errors)
            );
            return Result.Fail("Failed to authenticate with CIMAS pricing API");
        }

        if (string.IsNullOrEmpty(tokenResult.Data))
        {
            _logger.LogError("Received empty pricing access token from CIMAS");
            return Result.Fail("Received invalid pricing token from CIMAS");
        }

        _cache.Set(PRICING_TOKEN_KEY, tokenResult.Data, PricingTokenCacheExpiry);
        _logger.LogInformation("Successfully obtained new pricing access token");
        return Result.Ok(true);
    }

    public async Task<Result<TariffResponse>> GetTariffByCodeAsync(string tariffCode)
    {
        var authResult = await EnsurePricingAuthenticationAsync();
        if (!authResult.Success)
        {
            return Result.Fail(authResult.Errors);
        }

        var pricingToken = _cache.Get<string>(PRICING_TOKEN_KEY)!;
        var input = new TariffLookupInput(tariffCode, pricingToken);

        return await _cimasProvider.GetTariffByCodeAsync(input);
    }
}
