using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Claims.Providers.CIMAS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Jude.Server.Domains.Claims;

public interface IClaimsService
{
    Task<Result<Member>> GetMemberAsync(int membershipNumber, int suffix);
    Task<Result<List<ClaimResponse>>> GetPastClaimsAsync(string practiceNumber);
    Task<Result<ClaimResponse>> SubmitClaimAsync(ClaimRequest request);
    Task<Result<bool>> ReverseClaimAsync(string transactionNumber);
    Task<Result<bool>> UpdateClaimAsync(ClaimModel claim);
    Task<Result<bool>> UpdateAgentReview(AgentReviewModel review);
    Task<Result<GetClaimsResponse>> GetClaimsAsync(GetClaimsRequest request);
    Task<Result<GetClaimDetailResponse>> GetClaimAsync(Guid claimId);
    Task<Result<ClaimsDashboardResponse>> GetDashboardStatsAsync(ClaimsDashboardRequest request);
    Task<Result<TariffResponse>> GetTariffByCodeAsync(string tariffCode);
    Task<Result<List<TariffResponse>>> GetTariffsByCodesAsync(string[] tariffCodes);
    Task<Result<bool>> UpdateClaimStatus(Guid ClaimId, ClaimStatus status);
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
    private static readonly TimeSpan PricingTokenCacheExpiry = TimeSpan.FromMinutes(50); // Pricing tokens usually expir

    // e in 60 minutes

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

    public async Task<Result<GetClaimsResponse>> GetClaimsAsync(GetClaimsRequest request)
    {
        try
        {
            var query = _repository.Claims.AsQueryable();

            if (request.Status != null && request.Status.Length > 0)
            {
                query = query.Where(c => request.Status.Contains(c.Status));
            }

            var totalCount = await query.CountAsync();

            var claims = await query
                .OrderByDescending(c => c.IngestedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new GetClaimResponse(
                    c.Id,
                    c.TransactionNumber,
                    c.ClaimNumber,
                    c.PatientFirstName,
                    c.PatientSurname,
                    c.MedicalSchemeName,
                    c.TotalClaimAmount,
                    c.Status,
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

    public async Task<Result<GetClaimDetailResponse>> GetClaimAsync(Guid claimId)
    {
        try
        {
            var claim = await _repository
                .Claims.Where(c => c.Id == claimId)
                .Select(c => new GetClaimDetailResponse(
                    c.Id,
                    c.IngestedAt,
                    c.UpdatedAt,
                    c.Status,
                    c.Data,
                    c.TransactionNumber,
                    c.ClaimNumber,
                    c.PatientFirstName,
                    c.PatientSurname,
                    c.MedicalSchemeName,
                    c.TotalClaimAmount,
                    c.AgentReview != null
                        ? new AgentReviewResponse(
                            c.AgentReview.Id,
                            c.AgentReview.ReviewedAt,
                            c.AgentReview.Decision,
                            c.AgentReview.Recommendation,
                            c.AgentReview.Reasoning,
                            c.AgentReview.ConfidenceScore
                        )
                        : null,
                    c.HumanReview != null
                        ? new HumanReviewResponse(
                            c.HumanReview.Id,
                            c.HumanReview.ReviewedAt,
                            c.HumanReview.Decision,
                            c.HumanReview.Comments
                        )
                        : null,
                    c.ReviewedBy != null
                        ? new ReviewerInfo(
                            c.ReviewedBy.Id,
                            c.ReviewedBy.Username,
                            c.ReviewedBy.Email
                        )
                        : null
                ))
                .FirstOrDefaultAsync();

            if (claim == null)
            {
                return Result.Fail("Claim not found");
            }

            return Result.Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving claim {ClaimId}", claimId);
            return Result.Fail($"Failed to retrieve claim: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateAgentReview(AgentReviewModel review)
    {
        await _repository.AgentReviews.AddAsync(review);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<Result<bool>> UpdateClaimStatus(Guid claimId, ClaimStatus status)
    {
        var claim = await _repository.Claims.FirstOrDefaultAsync(c => c.Id == claimId);
        if (claim == null)
            return Result.Fail($"Claim with {claimId} not found");
        claim.Status = status;
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<Result<bool>> UpdateClaimAsync(ClaimModel claim)
    {
        try
        {
            claim.UpdatedAt = DateTime.UtcNow;
            _repository.Claims.Update(claim);
            await _repository.SaveChangesAsync();
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

    private double CalculateChangePercent(double current, double previous)
    {
        if (previous == 0)
        {
            return current > 0 ? 100.0 : 0.0;
        }
        return (current - previous) / previous * 100;
    }

    public async Task<Result<ClaimsDashboardResponse>> GetDashboardStatsAsync(
        ClaimsDashboardRequest request
    )
    {
        try
        {
            var now = DateTime.UtcNow;
            DateTime currentPeriodStart,
                previousPeriodStart,
                previousPeriodEnd;

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

            var currentPeriodQuery = _repository.Claims.Where(c =>
                c.IngestedAt >= currentPeriodStart && c.IngestedAt <= now
            );
            var previousPeriodQuery = _repository.Claims.Where(c =>
                c.IngestedAt >= previousPeriodStart && c.IngestedAt < previousPeriodEnd
            );

            // --- Current Period Stats ---
            int totalClaims = await currentPeriodQuery.CountAsync();
            int autoApprove = await currentPeriodQuery
                .Include(c => c.AgentReview)
                .CountAsync(c =>
                    c.AgentReview != null
                    && c.AgentReview.Decision == ClaimDecision.Approve
                    && c.AgentReview.Recommendation == "Auto-Approve"
                );
            var processingTimes = await currentPeriodQuery
                .Include(c => c.AgentReview)
                .Where(c => c.AgentReview != null)
                .Select(c => new { c.IngestedAt, c.AgentReview!.ReviewedAt })
                .ToListAsync();
            int claimsFlagged = await currentPeriodQuery
                .Include(c => c.AgentReview)
                .CountAsync(c =>
                    c.AgentReview != null && c.AgentReview.Decision == ClaimDecision.Reject
                );

            // --- Previous Period Stats ---
            int prevTotalClaims = await previousPeriodQuery.CountAsync();
            int prevAutoApprove = await previousPeriodQuery
                .Include(c => c.AgentReview)
                .CountAsync(c =>
                    c.AgentReview != null
                    && c.AgentReview.Decision == ClaimDecision.Approve
                    && c.AgentReview.Recommendation == "Auto-Approve"
                );
            var prevProcessingTimes = await previousPeriodQuery
                .Include(c => c.AgentReview)
                .Where(c => c.AgentReview != null)
                .Select(c => new { c.IngestedAt, c.AgentReview!.ReviewedAt })
                .ToListAsync();
            int prevClaimsFlagged = await previousPeriodQuery
                .Include(c => c.AgentReview)
                .CountAsync(c =>
                    c.AgentReview != null && c.AgentReview.Decision == ClaimDecision.Reject
                );

            // --- Calculate Final Stats ---
            double avgProcessingTime = processingTimes.Any()
                ? processingTimes.Average(c => (c.ReviewedAt - c.IngestedAt).TotalMinutes)
                : 0;
            double prevAvgProcessingTime = prevProcessingTimes.Any()
                ? prevProcessingTimes.Average(c => (c.ReviewedAt - c.IngestedAt).TotalMinutes)
                : 0;

            double autoApproveRate = totalClaims > 0 ? (double)autoApprove / totalClaims * 100 : 0;
            double prevAutoApproveRate =
                prevTotalClaims > 0 ? (double)prevAutoApprove / prevTotalClaims * 100 : 0;

            // --- Calculate Change Percentages ---
            double totalClaimsChangePercent = CalculateChangePercent(totalClaims, prevTotalClaims);
            double autoApproveRateChangePercent = CalculateChangePercent(
                autoApproveRate,
                prevAutoApproveRate
            );
            double avgProcessingTimeChangePercent = CalculateChangePercent(
                avgProcessingTime,
                prevAvgProcessingTime
            );
            double claimsFlaggedChangePercent = CalculateChangePercent(
                claimsFlagged,
                prevClaimsFlagged
            );

            // --- Activity Chart Data ---
            var activity = await GetClaimsActivityAsync(
                currentPeriodQuery,
                request.Period,
                currentPeriodStart
            );

            var response = new ClaimsDashboardResponse(
                totalClaims,
                autoApproveRate,
                avgProcessingTime,
                claimsFlagged,
                totalClaimsChangePercent,
                autoApproveRateChangePercent,
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

    private async Task<List<ClaimsActivityResponse>> GetClaimsActivityAsync(
        IQueryable<ClaimModel> query,
        ClaimsDashboardPeriod period,
        DateTime startDate
    )
    {
        var activity = new List<ClaimsActivityResponse>();

        switch (period)
        {
            case ClaimsDashboardPeriod.Last7Days:
                var dailyData = await query
                    .Include(c => c.AgentReview)
                    .Include(c => c.HumanReview)
                    .GroupBy(c => c.IngestedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Total = g.Count(),
                        Processed = g.Count(c => c.Status != ClaimStatus.Pending),
                        Approve = g.Count(c =>
                            (
                                c.AgentReview != null
                                && c.AgentReview.Decision == ClaimDecision.Approve
                            )
                            || (
                                c.HumanReview != null
                                && c.HumanReview.Decision == ClaimDecision.Approve
                            )
                        ),
                        Reject = g.Count(c =>
                            (
                                c.AgentReview != null
                                && c.AgentReview.Decision == ClaimDecision.Reject
                            )
                            || (
                                c.HumanReview != null
                                && c.HumanReview.Decision == ClaimDecision.Reject
                            )
                        ),
                    })
                    .ToDictionaryAsync(x => x.Date, x => x);

                for (int i = 0; i < 7; i++)
                {
                    var date = startDate.AddDays(i).Date;
                    if (dailyData.TryGetValue(date, out var data))
                    {
                        activity.Add(
                            new ClaimsActivityResponse(
                                date.ToString("ddd"),
                                data.Total,
                                data.Processed,
                                data.Approve,
                                data.Reject
                            )
                        );
                    }
                    else
                    {
                        activity.Add(new ClaimsActivityResponse(date.ToString("ddd"), 0, 0, 0, 0));
                    }
                }
                break;

            case ClaimsDashboardPeriod.Last30Days:
                var monthlyData = await query
                    .Include(c => c.AgentReview)
                    .Include(c => c.HumanReview)
                    .GroupBy(c => c.IngestedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Total = g.Count(),
                        Processed = g.Count(c => c.Status != ClaimStatus.Pending),
                        Approve = g.Count(c =>
                            (
                                c.AgentReview != null
                                && c.AgentReview.Decision == ClaimDecision.Approve
                            )
                            || (
                                c.HumanReview != null
                                && c.HumanReview.Decision == ClaimDecision.Approve
                            )
                        ),
                        Reject = g.Count(c =>
                            (
                                c.AgentReview != null
                                && c.AgentReview.Decision == ClaimDecision.Reject
                            )
                            || (
                                c.HumanReview != null
                                && c.HumanReview.Decision == ClaimDecision.Reject
                            )
                        ),
                    })
                    .ToDictionaryAsync(x => x.Date, x => x);

                for (int i = 0; i < 30; i++)
                {
                    var date = startDate.AddDays(i).Date;
                    if (monthlyData.TryGetValue(date, out var data))
                    {
                        activity.Add(
                            new ClaimsActivityResponse(
                                date.ToString("MM-dd"),
                                data.Total,
                                data.Processed,
                                data.Approve,
                                data.Reject
                            )
                        );
                    }
                    else
                    {
                        activity.Add(
                            new ClaimsActivityResponse(date.ToString("MM-dd"), 0, 0, 0, 0)
                        );
                    }
                }
                break;

            case ClaimsDashboardPeriod.LastQuarter:
                var quarterlyData = await query
                    .Include(c => c.AgentReview)
                    .Include(c => c.HumanReview)
                    .GroupBy(c => new { c.IngestedAt.Year, c.IngestedAt.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Total = g.Count(),
                        Processed = g.Count(c => c.Status != ClaimStatus.Pending),
                        Approve = g.Count(c =>
                            (
                                c.AgentReview != null
                                && c.AgentReview.Decision == ClaimDecision.Approve
                            )
                            || (
                                c.HumanReview != null
                                && c.HumanReview.Decision == ClaimDecision.Approve
                            )
                        ),
                        Reject = g.Count(c =>
                            (
                                c.AgentReview != null
                                && c.AgentReview.Decision == ClaimDecision.Reject
                            )
                            || (
                                c.HumanReview != null
                                && c.HumanReview.Decision == ClaimDecision.Reject
                            )
                        ),
                    })
                    .ToDictionaryAsync(x => new { x.Year, x.Month }, x => x);

                for (int i = 0; i < 4; i++)
                {
                    var date = startDate.AddMonths(i);
                    if (quarterlyData.TryGetValue(new { date.Year, date.Month }, out var data))
                    {
                        activity.Add(
                            new ClaimsActivityResponse(
                                date.ToString("MMM yyyy"),
                                data.Total,
                                data.Processed,
                                data.Approve,
                                data.Reject
                            )
                        );
                    }
                    else
                    {
                        activity.Add(
                            new ClaimsActivityResponse(date.ToString("MMM yyyy"), 0, 0, 0, 0)
                        );
                    }
                }
                break;

            case ClaimsDashboardPeriod.Last24Hours:
                var hourlyData = await query
                    .Include(c => c.AgentReview)
                    .Include(c => c.HumanReview)
                    .Where(c => c.IngestedAt.Date == DateTime.UtcNow.Date)
                    .GroupBy(c => c.IngestedAt.Hour)
                    .Select(g => new
                    {
                        Hour = g.Key,
                        Total = g.Count(),
                        Processed = g.Count(c => c.Status != ClaimStatus.Pending),
                        Approve = g.Count(c =>
                            (
                                c.AgentReview != null
                                && c.AgentReview.Decision == ClaimDecision.Approve
                            )
                            || (
                                c.HumanReview != null
                                && c.HumanReview.Decision == ClaimDecision.Approve
                            )
                        ),
                        Reject = g.Count(c =>
                            (
                                c.AgentReview != null
                                && c.AgentReview.Decision == ClaimDecision.Reject
                            )
                            || (
                                c.HumanReview != null
                                && c.HumanReview.Decision == ClaimDecision.Reject
                            )
                        ),
                    })
                    .ToDictionaryAsync(x => x.Hour, x => x);

                for (int i = 0; i < 24; i++)
                {
                    if (hourlyData.TryGetValue(i, out var data))
                    {
                        activity.Add(
                            new ClaimsActivityResponse(
                                i.ToString("D2") + ":00",
                                data.Total,
                                data.Processed,
                                data.Approve,
                                data.Reject
                            )
                        );
                    }
                    else
                    {
                        activity.Add(
                            new ClaimsActivityResponse(i.ToString("D2") + ":00", 0, 0, 0, 0)
                        );
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

    public async Task<Result<List<TariffResponse>>> GetTariffsByCodesAsync(string[] tariffCodes)
    {
        var authResult = await EnsurePricingAuthenticationAsync();
        if (!authResult.Success)
        {
            return Result.Fail(authResult.Errors);
        }

        var pricingToken = _cache.Get<string>(PRICING_TOKEN_KEY)!;
        var results = new List<TariffResponse>();

        foreach (var tariffCode in tariffCodes)
        {
            if (string.IsNullOrWhiteSpace(tariffCode))
                continue;

            var input = new TariffLookupInput(tariffCode, pricingToken);
            var result = await _cimasProvider.GetTariffByCodeAsync(input);

            if (result.Success && result.Data != null)
            {
                results.Add(result.Data);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to get tariff for code {TariffCode}: {Error}",
                    tariffCode,
                    string.Join(", ", result.Errors)
                );
            }
        }

        return Result.Ok(results);
    }
}
