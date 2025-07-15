using Jude.Server.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jude.Server.Domains.Stats;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly IStatsService _statsService;
    private readonly ILogger<StatsController> _logger;

    public StatsController(IStatsService statsService, ILogger<StatsController> logger)
    {
        _statsService = statsService;
        _logger = logger;
    }

    [HttpGet("dashboard/metrics")]
    public async Task<IActionResult> GetDashboardMetrics([FromQuery] GetDashboardMetricsRequest request)
    {
        _logger.LogInformation("Getting dashboard metrics for date range: {DateRange}", request.DateRange);
        
        var result = await _statsService.GetDashboardMetricsAsync(request);
        
        if (!result.Success)
        {
            _logger.LogWarning("Failed to get dashboard metrics: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }

    [HttpGet("dashboard/charts")]
    public async Task<IActionResult> GetDashboardCharts([FromQuery] GetDashboardChartsRequest request)
    {
        _logger.LogInformation("Getting dashboard charts for date range: {DateRange}", request.DateRange);
        
        var result = await _statsService.GetDashboardChartsAsync(request);
        
        if (!result.Success)
        {
            _logger.LogWarning("Failed to get dashboard charts: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }

    [HttpGet("dashboard/recent-claims")]
    public async Task<IActionResult> GetDashboardRecentClaims([FromQuery] GetDashboardRecentClaimsRequest request)
    {
        _logger.LogInformation("Getting recent claims for page {Page}, pageSize {PageSize}", request.Page, request.PageSize);
        
        var result = await _statsService.GetDashboardRecentClaimsAsync(request);
        
        if (!result.Success)
        {
            _logger.LogWarning("Failed to get recent claims: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }
}