using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Agents.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Jude.Server.Domains.Claims;

public interface IClaimsService
{
    Task<Result<bool>> UpdateClaimAsync(ClaimModel claim);
    Task<Result<bool>> UpdateAgentReview(Guid claimId, AgentReviewModel review);
    Task<Result<GetClaimsResponse>> GetClaimsAsync(GetClaimsRequest request);
    Task<Result<GetClaimDetailResponse>> GetClaimAsync(Guid claimId);
    Task<Result<ClaimsDashboardResponse>> GetDashboardStatsAsync(ClaimsDashboardRequest request);
    Task<Result<bool>> UpdateClaimStatus(Guid ClaimId, ClaimStatus status);
    Task<Result<UploadExcelResponse>> ProcessExcelUploadAsync(Stream excelStream, string fileName);
    Task<Result<SubmitHumanReviewResponse>> SubmitHumanReviewAsync(Guid claimId, Guid userId, SubmitHumanReviewRequest request);
}

public class ClaimsService : IClaimsService
{
    private readonly JudeDbContext _repository;
    private readonly ILogger<ClaimsService> _logger;
    private readonly IExcelClaimParser _excelParser;
    private readonly IClaimBulkInsertEventsQueue _bulkInsertQueue;

    public ClaimsService(
        JudeDbContext repository,
        ILogger<ClaimsService> logger,
        IExcelClaimParser excelParser,
        IClaimBulkInsertEventsQueue bulkInsertQueue
    )
    {
        _repository = repository;
        _logger = logger;
        _excelParser = excelParser;
        _bulkInsertQueue = bulkInsertQueue;
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
                    c.ClaimNumber,
                    c.PatientFirstName,
                    c.PatientSurname,
                    c.MemberNumber,
                    c.MedicalSchemeName,
                    c.OptionName,
                    c.PayerName,
                    c.TotalClaimAmount,
                    c.TotalAmountPaid,
                    c.CoPayAmount,
                    c.ProviderName,
                    c.PracticeNumber,
                    c.InvoiceReference,
                    c.ServiceDate,
                    c.AssessmentDate,
                    c.DateReceived,
                    c.ClaimCode,
                    c.CodeDescription,
                    c.Units,
                    c.AssessorName,
                    c.ClaimTypeCode,
                    c.PatientBirthDate,
                    c.PatientCurrentAge,
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
                        : null,
                    c.Source,
                    c.ClaimMarkdown
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

    public async Task<Result<bool>> UpdateAgentReview(Guid claimId, AgentReviewModel review)
    {
        var claim = await _repository.Claims.FirstOrDefaultAsync(c => c.Id == claimId);
        if (claim == null)
            return Result.Fail($"Claim with id {claimId} not found");

        review.ClaimId = claimId;
        await _repository.AgentReviews.AddAsync(review);
        await _repository.SaveChangesAsync();

        return Result.Ok(true);
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

    public async Task<Result<UploadExcelResponse>> ProcessExcelUploadAsync(
        Stream excelStream,
        string fileName
    )
    {
        _logger.LogInformation(
            "Starting Excel upload processing for file: {FileName}",
            fileName
        );

        var parseResult = await _excelParser.ParseExcelAsync(excelStream);

        if (!parseResult.Success)
        {
            return Result.Fail(
                parseResult.Errors.FirstOrDefault() ?? "Failed to parse Excel file"
            );
        }

        var claims = parseResult.Data!;

        _logger.LogInformation(
            "Parsed {Count} claims from Excel. Queueing for bulk insert.",
            claims.Count
        );

        var bulkInsertEvent = new ClaimBulkInsertEvent(claims, fileName, DateTime.UtcNow);
        await _bulkInsertQueue.Writer.WriteAsync(bulkInsertEvent);

        _logger.LogInformation(
            "Excel upload complete. {Count} claims queued for processing by background service.",
            claims.Count
        );

        var response = new UploadExcelResponse(
            TotalRows: claims.Count,
            SuccessfullyQueued: 0,
            Duplicates: 0,
            Failed: 0,
            Errors: new List<string>()
        );

        return Result.Ok(response);
    }

    public async Task<Result<SubmitHumanReviewResponse>> SubmitHumanReviewAsync(
        Guid claimId,
        Guid userId,
        SubmitHumanReviewRequest request
    )
    {
        var claim = await _repository.Claims.Include(c => c.HumanReview).FirstOrDefaultAsync(c =>
            c.Id == claimId
        );

        if (claim == null)
            return Result.Fail($"Claim with id {claimId} not found");

        if (string.IsNullOrWhiteSpace(request.Comments))
            return Result.Fail("Comments are required for human review");

        if (request.Decision == ClaimDecision.None)
            return Result.Fail("A decision (Approve or Reject) must be made");

        var humanReview = new HumanReviewModel
        {
            Id = Guid.NewGuid(),
            ClaimId = claimId,
            ReviewedAt = DateTime.UtcNow,
            Decision = request.Decision,
            Comments = request.Comments,
        };

        if (claim.HumanReview != null)
        {
            _repository.HumanReviews.Remove(claim.HumanReview);
        }

        await _repository.HumanReviews.AddAsync(humanReview);

        claim.ReviewedById = userId;
        claim.Status =
            request.Decision == ClaimDecision.Approve
                ? ClaimStatus.Approved
                : ClaimStatus.Rejected;
        claim.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        _logger.LogInformation(
            "Human review submitted for claim {ClaimId} by user {UserId} with decision {Decision}",
            claimId,
            userId,
            request.Decision
        );

        var response = new SubmitHumanReviewResponse(
            humanReview.Id,
            humanReview.ReviewedAt,
            humanReview.Decision,
            humanReview.Comments
        );

        return Result.Ok(response);
    }
}
