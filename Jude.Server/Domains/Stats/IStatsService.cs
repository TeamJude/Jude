using Jude.Server.Core.Helpers;

namespace Jude.Server.Domains.Stats;

public interface IStatsService
{
    Task<Result<DashboardMetricsResponse>> GetDashboardMetricsAsync(GetDashboardMetricsRequest request);
    Task<Result<DashboardChartsResponse>> GetDashboardChartsAsync(GetDashboardChartsRequest request);
    Task<Result<DashboardRecentClaimsResponse>> GetDashboardRecentClaimsAsync(GetDashboardRecentClaimsRequest request);
}