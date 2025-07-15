using System.Globalization;
using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Domains.Stats;

public class StatsService : IStatsService
{
    private readonly JudeDbContext _context;
    private readonly ILogger<StatsService> _logger;

    public StatsService(JudeDbContext context, ILogger<StatsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<DashboardMetricsResponse>> GetDashboardMetricsAsync(GetDashboardMetricsRequest request)
    {
        try
        {
            var (startDate, endDate) = GetDateRange(request.DateRange, request.StartDate, request.EndDate);
            var (prevStartDate, prevEndDate) = GetPreviousDateRange(request.DateRange, startDate, endDate);

            // Current period metrics
            var currentClaims = await _context.Claims
                .Where(c => c.IngestedAt >= startDate && c.IngestedAt <= endDate)
                .ToListAsync();

            // Previous period metrics for comparison
            var previousClaims = await _context.Claims
                .Where(c => c.IngestedAt >= prevStartDate && c.IngestedAt < startDate)
                .ToListAsync();

            // Calculate metrics
            var totalClaims = CalculateTotalClaims(currentClaims, previousClaims);
            var autoApprovedRate = CalculateAutoApprovedRate(currentClaims, previousClaims);
            var avgProcessingTime = CalculateAverageProcessingTime(currentClaims, previousClaims);
            var claimsFlagged = CalculateClaimsFlagged(currentClaims, previousClaims);

            var response = new DashboardMetricsResponse(
                totalClaims,
                autoApprovedRate,
                avgProcessingTime,
                claimsFlagged
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard metrics");
            return Result.Fail("Failed to retrieve dashboard metrics");
        }
    }

    public async Task<Result<DashboardChartsResponse>> GetDashboardChartsAsync(GetDashboardChartsRequest request)
    {
        try
        {
            var (startDate, endDate) = GetDateRange(request.DateRange, request.StartDate, request.EndDate);

            var claims = await _context.Claims
                .Where(c => c.IngestedAt >= startDate && c.IngestedAt <= endDate)
                .ToListAsync();

            // Claims Activity Data (daily breakdown)
            var claimsActivity = GenerateClaimsActivityData(claims, startDate, endDate, request.DateRange);

            // Distribution charts
            var claimCategories = GenerateClaimCategoriesData(claims);
            var claimSources = GenerateClaimSourcesData(claims);
            var adjudicationTypes = GenerateAdjudicationTypesData(claims);

            var response = new DashboardChartsResponse(
                claimsActivity,
                claimCategories,
                claimSources,
                adjudicationTypes
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard charts");
            return Result.Fail("Failed to retrieve dashboard charts");
        }
    }

    public async Task<Result<DashboardRecentClaimsResponse>> GetDashboardRecentClaimsAsync(GetDashboardRecentClaimsRequest request)
    {
        try
        {
            var (startDate, endDate) = GetDateRange(request.DateRange, request.StartDate, request.EndDate);

            var query = _context.Claims
                .Where(c => c.IngestedAt >= startDate && c.IngestedAt <= endDate);

            // Apply status filter
            if (request.Status != null && request.Status.Length > 0)
            {
                query = query.Where(c => request.Status.Contains(c.Status));
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(c =>
                    c.TransactionNumber.ToLower().Contains(searchLower) ||
                    c.ProviderPractice.ToLower().Contains(searchLower) ||
                    c.PatientName.ToLower().Contains(searchLower));
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Get paginated results
            var claims = await query
                .OrderByDescending(c => c.IngestedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var recentClaims = claims.Select(c => new RecentClaimInfo(
                c.TransactionNumber,
                c.ProviderPractice,
                c.PatientName,
                c.ClaimAmount.ToString("C", CultureInfo.CurrentCulture),
                MapClaimStatusToFrontend(c.Status, c.FinalDecision, c.IsFlagged),
                c.IsFlagged ? GetFlagReason(c) : null,
                c.SubmittedAt ?? c.IngestedAt
            )).ToArray();

            var response = new DashboardRecentClaimsResponse(recentClaims, totalCount);

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent claims");
            return Result.Fail("Failed to retrieve recent claims");
        }
    }

    private static (DateTime startDate, DateTime endDate) GetDateRange(DateRangeFilter dateRange, DateTime? startDate, DateTime? endDate)
    {
        var now = DateTime.UtcNow;
        
        return dateRange switch
        {
            DateRangeFilter.Today => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
            DateRangeFilter.Week => (now.AddDays(-7), now),
            DateRangeFilter.Month => (now.AddDays(-30), now),
            DateRangeFilter.Quarter => (now.AddDays(-90), now),
            DateRangeFilter.Custom when startDate.HasValue && endDate.HasValue => 
                (startDate.Value.Date, endDate.Value.Date.AddDays(1).AddTicks(-1)),
            _ => (now.AddDays(-7), now) // Default to week
        };
    }

    private static (DateTime startDate, DateTime endDate) GetPreviousDateRange(DateRangeFilter dateRange, DateTime currentStart, DateTime currentEnd)
    {
        var duration = currentEnd - currentStart;
        return (currentStart - duration, currentStart);
    }

    private static MetricInfo CalculateTotalClaims(List<ClaimModel> current, List<ClaimModel> previous)
    {
        var currentCount = current.Count;
        var previousCount = previous.Count;
        var change = previousCount > 0 ? ((double)(currentCount - previousCount) / previousCount) * 100 : 0;

        return new MetricInfo(currentCount.ToString("N0"), change);
    }

    private static MetricInfo CalculateAutoApprovedRate(List<ClaimModel> current, List<ClaimModel> previous)
    {
        var currentAutoApproved = current.Count(c => c.FinalDecision == ClaimDecision.Approved && !c.RequiresHumanReview);
        var currentTotal = current.Count;
        var currentRate = currentTotal > 0 ? (double)currentAutoApproved / currentTotal * 100 : 0;

        var previousAutoApproved = previous.Count(c => c.FinalDecision == ClaimDecision.Approved && !c.RequiresHumanReview);
        var previousTotal = previous.Count;
        var previousRate = previousTotal > 0 ? (double)previousAutoApproved / previousTotal * 100 : 0;

        var change = previousRate > 0 ? (currentRate - previousRate) / previousRate * 100 : 0;

        return new MetricInfo($"{currentRate:F1}%", change);
    }

    private static MetricInfo CalculateAverageProcessingTime(List<ClaimModel> current, List<ClaimModel> previous)
    {
        var currentProcessedClaims = current.Where(c => c.ProcessedAt.HasValue && c.SubmittedAt.HasValue).ToList();
        var currentAvgMinutes = currentProcessedClaims.Count > 0
            ? currentProcessedClaims.Average(c => (c.ProcessedAt!.Value - c.SubmittedAt!.Value).TotalMinutes)
            : 0;

        var previousProcessedClaims = previous.Where(c => c.ProcessedAt.HasValue && c.SubmittedAt.HasValue).ToList();
        var previousAvgMinutes = previousProcessedClaims.Count > 0
            ? previousProcessedClaims.Average(c => (c.ProcessedAt!.Value - c.SubmittedAt!.Value).TotalMinutes)
            : 0;

        var change = previousAvgMinutes > 0 ? (currentAvgMinutes - previousAvgMinutes) / previousAvgMinutes * 100 : 0;

        return new MetricInfo(currentAvgMinutes.ToString("F1"), change, "minutes");
    }

    private static MetricInfo CalculateClaimsFlagged(List<ClaimModel> current, List<ClaimModel> previous)
    {
        var currentFlagged = current.Count(c => c.IsFlagged);
        var previousFlagged = previous.Count(c => c.IsFlagged);
        var change = previousFlagged > 0 ? ((double)(currentFlagged - previousFlagged) / previousFlagged) * 100 : 0;

        return new MetricInfo(currentFlagged.ToString("N0"), change);
    }

    private static ClaimsActivityData[] GenerateClaimsActivityData(List<ClaimModel> claims, DateTime startDate, DateTime endDate, DateRangeFilter dateRange)
    {
        var result = new List<ClaimsActivityData>();
        
        // Determine grouping interval
        var interval = dateRange switch
        {
            DateRangeFilter.Today => TimeSpan.FromHours(1),
            DateRangeFilter.Week => TimeSpan.FromDays(1),
            DateRangeFilter.Month => TimeSpan.FromDays(1),
            DateRangeFilter.Quarter => TimeSpan.FromDays(7),
            _ => TimeSpan.FromDays(1)
        };

        var current = startDate;
        while (current <= endDate)
        {
            var nextPeriod = current.Add(interval);
            var periodClaims = claims.Where(c => c.IngestedAt >= current && c.IngestedAt < nextPeriod).ToList();

            var dateLabel = dateRange == DateRangeFilter.Today 
                ? current.ToString("HH:mm")
                : current.ToString("MMM dd");

            result.Add(new ClaimsActivityData(
                dateLabel,
                periodClaims.Count,
                periodClaims.Count(c => c.Status == ClaimStatus.Completed || c.Status == ClaimStatus.Review),
                periodClaims.Count(c => c.FinalDecision == ClaimDecision.Approved),
                periodClaims.Count(c => c.FinalDecision == ClaimDecision.Rejected)
            ));

            current = nextPeriod;
        }

        return result.ToArray();
    }

    private static ChartDistributionData GenerateClaimCategoriesData(List<ClaimModel> claims)
    {
        // Since we don't have specific categories in the model, we'll simulate based on amount ranges
        var data = new[]
        {
            new ChartDataPoint("Outpatient", claims.Count(c => c.ClaimAmount <= 1000)),
            new ChartDataPoint("Inpatient", claims.Count(c => c.ClaimAmount > 1000 && c.ClaimAmount <= 5000)),
            new ChartDataPoint("Pharmacy", claims.Count(c => c.ClaimAmount > 100 && c.ClaimAmount <= 500)),
            new ChartDataPoint("Dental", claims.Count(c => c.ClaimAmount > 50 && c.ClaimAmount <= 300))
        };

        return new ChartDistributionData("Claim Categories", data);
    }

    private static ChartDistributionData GenerateClaimSourcesData(List<ClaimModel> claims)
    {
        var data = new[]
        {
            new ChartDataPoint("Providers", claims.Count(c => c.Source == ClaimSource.CIMAS)),
            new ChartDataPoint("Members", 0), // Not available in current model
            new ChartDataPoint("Partners", 0), // Not available in current model
            new ChartDataPoint("Other", 0) // Not available in current model
        };

        return new ChartDistributionData("Claim Sources", data);
    }

    private static ChartDistributionData GenerateAdjudicationTypesData(List<ClaimModel> claims)
    {
        var data = new[]
        {
            new ChartDataPoint("Auto-Approved", claims.Count(c => c.FinalDecision == ClaimDecision.Approved && !c.RequiresHumanReview)),
            new ChartDataPoint("Manual Review", claims.Count(c => c.RequiresHumanReview && c.Status != ClaimStatus.Completed)),
            new ChartDataPoint("Flagged", claims.Count(c => c.IsFlagged)),
            new ChartDataPoint("Rejected", claims.Count(c => c.FinalDecision == ClaimDecision.Rejected))
        };

        return new ChartDistributionData("Adjudication Types", data);
    }

    private static string MapClaimStatusToFrontend(ClaimStatus status, ClaimDecision? decision, bool isFlagged)
    {
        if (isFlagged) return "flagged";
        
        return status switch
        {
            ClaimStatus.Completed when decision == ClaimDecision.Approved => "auto-approved",
            ClaimStatus.Review => "under-review",
            ClaimStatus.Pending or ClaimStatus.Processing => "pending",
            _ => "pending"
        };
    }

    private static string GetFlagReason(ClaimModel claim)
    {
        return claim.FraudRiskLevel switch
        {
            FraudRiskLevel.High or FraudRiskLevel.Critical => "High Risk",
            FraudRiskLevel.Medium => "Unusual Pattern",
            _ => "Review Required"
        };
    }
}