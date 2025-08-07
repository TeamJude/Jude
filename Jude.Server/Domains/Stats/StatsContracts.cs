using Jude.Server.Data.Models;

namespace Jude.Server.Domains.Stats;

public enum DateRangeFilter
{
    Today,
    Week,
    Month,
    Quarter,
    Custom
}

public record GetDashboardMetricsRequest(
    DateRangeFilter DateRange = DateRangeFilter.Week,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);

public record GetDashboardChartsRequest(
    DateRangeFilter DateRange = DateRangeFilter.Week,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);

public record GetDashboardRecentClaimsRequest(
    int Page = 1,
    int PageSize = 10,
    ClaimStatus[]? Status = null,
    string? Search = null,
    DateRangeFilter DateRange = DateRangeFilter.Week,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);

// Response DTOs
public record DashboardMetricsResponse(
    MetricInfo TotalClaims,
    MetricInfo AutoApprovedRate,
    MetricInfo AverageProcessingTime,
    MetricInfo ClaimsFlagged
);

public record MetricInfo(
    string Value,
    double? ChangePercentage,
    string? Subtitle = null
);

public record DashboardChartsResponse(
    ClaimsActivityData[] ClaimsActivity,
    ChartDistributionData ClaimCategories,
    ChartDistributionData ClaimSources,
    ChartDistributionData AdjudicationTypes
);

public record ClaimsActivityData(
    string Date,
    int NewClaims,
    int Processed,
    int Approved,
    int Rejected
);

public record ChartDistributionData(
    string Title,
    ChartDataPoint[] Data
);

public record ChartDataPoint(
    string Name,
    int Value
);

public record DashboardRecentClaimsResponse(
    RecentClaimInfo[] Claims,
    int TotalCount
);

public record RecentClaimInfo(
    string Id,
    string Provider,
    string Member,
    string Amount,
    string Status,
    string? Flag,
    DateTime SubmittedAt
);