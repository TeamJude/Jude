using System.Text.Json;
using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Claims.Providers.CIMAS;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Domains.Claims;

public interface IClaimsService
{
    Task<Result<ClaimModel>> CreateClaimFromCIMAS(CreateClaimFromCIMASRequest request, Guid userId);
    Task<Result<ClaimDetailDto>> GetClaim(Guid claimId);
    Task<Result<GetClaimsResponse>> GetClaims(GetClaimsRequest request);
    Task<Result<ClaimModel>> UpdateClaimStatus(Guid claimId, UpdateClaimStatusRequest request, Guid userId);
    Task<Result<ClaimModel>> ReviewClaim(Guid claimId, ReviewClaimRequest request, Guid userId);
    Task<Result<ClaimModel>> ProcessClaim(Guid claimId, ProcessClaimRequest request);
    Task<Result<ClaimModel[]>> BatchReviewClaims(BatchReviewClaimsRequest request, Guid userId);
    Task<Result<ClaimModel>> ResubmitClaim(ResubmitClaimRequest request, Guid userId);
}

public class ClaimsService : IClaimsService
{
    private readonly JudeDbContext _repository;
    private readonly ICIMASProvider _cimasProvider;
    private readonly ILogger<ClaimsService> _logger;

    public ClaimsService(
        JudeDbContext repository, 
        ICIMASProvider cimasProvider,
        ILogger<ClaimsService> logger)
    {
        _repository = repository;
        _cimasProvider = cimasProvider;
        _logger = logger;
    }

    public async Task<Result<ClaimModel>> CreateClaimFromCIMAS(CreateClaimFromCIMASRequest request, Guid userId)
    {
        try
        {
            // Extract key information from CIMAS request
            var patient = request.CIMASRequest.Patient;
            var provider = request.CIMASRequest.Provider;
            var header = request.CIMASRequest.ClaimHeader;
            var transaction = request.CIMASRequest.Transaction;

            var claim = new ClaimModel
            {
                ClaimNumber = header.ClaimNumber,
                TransactionNumber = request.CIMASResponse?.TransactionResponse?.Number ?? string.Empty,
                CIMASRequestPayload = JsonSerializer.Serialize(request.CIMASRequest),
                CIMASResponsePayload = request.CIMASResponse != null ? JsonSerializer.Serialize(request.CIMASResponse) : string.Empty,
                
                // Extract patient information
                MembershipNumber = request.CIMASRequest.Member.MedicalSchemeNumber,
                DependantCode = patient.DependantCode,
                PatientName = $"{patient.Personal.FirstName} {patient.Personal.Surname}".Trim(),
                PatientIdNumber = patient.Personal.IDNumber,
                
                // Extract provider information
                ProviderPracticeNumber = provider.PracticeNumber,
                ProviderPracticeName = provider.PracticeName,
                
                // Extract financial information
                ClaimAmount = header.TotalValues.GrossAmount / 100m, // Convert from cents
                ApprovedAmount = request.CIMASResponse?.ClaimHeaderResponse?.TotalValues?.GrossAmount / 100m ?? 0,
                PatientPayAmount = header.TotalValues.PatientPayAmount / 100m,
                Currency = request.CIMASRequest.Member.Currency,
                
                Status = ClaimStatus.Pending,
                Source = ClaimSource.CIMAS,
                SubmittedAt = DateTime.UtcNow,
                ProcessedById = userId,
                
                // Initialize audit log
                AuditLog = JsonSerializer.Serialize(new[]
                {
                    new { Action = "Created", Timestamp = DateTime.UtcNow, UserId = userId, Details = "Claim created from CIMAS" }
                })
            };

            await _repository.Claims.AddAsync(claim);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Created claim {ClaimId} for patient {PatientName}", claim.Id, claim.PatientName);

            return Result.Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating claim from CIMAS");
            return Result.Exception("Error creating claim from CIMAS");
        }
    }

    public async Task<Result<ClaimDetailDto>> GetClaim(Guid claimId)
    {
        try
        {
            var claim = await _repository.Claims
                .Include(c => c.ProcessedById != null ? _repository.Users.Where(u => u.Id == c.ProcessedById) : null)
                .Include(c => c.ReviewedById != null ? _repository.Users.Where(u => u.Id == c.ReviewedById) : null)
                .FirstOrDefaultAsync(c => c.Id == claimId);

            if (claim == null)
            {
                return Result.Fail("Claim not found");
            }

            var reviewedByUser = claim.ReviewedById.HasValue 
                ? await _repository.Users.FindAsync(claim.ReviewedById.Value)
                : null;

            var fraudIndicators = string.IsNullOrEmpty(claim.FraudIndicators) 
                ? Array.Empty<string>()
                : JsonSerializer.Deserialize<string[]>(claim.FraudIndicators) ?? Array.Empty<string>();

            var claimDetail = new ClaimDetailDto(
                claim.Id,
                claim.ClaimNumber,
                claim.TransactionNumber,
                claim.MembershipNumber,
                claim.DependantCode,
                claim.PatientName,
                claim.PatientIdNumber,
                claim.ProviderPracticeNumber,
                claim.ProviderPracticeName,
                claim.ClaimAmount,
                claim.ApprovedAmount,
                claim.PatientPayAmount,
                claim.Currency,
                claim.Status,
                claim.Source,
                claim.CreatedAt,
                claim.UpdatedAt,
                claim.SubmittedAt,
                claim.ProcessedAt,
                claim.AgentRecommendation,
                claim.AgentReasoning,
                claim.AgentConfidenceScore,
                claim.AgentProcessedAt,
                claim.IsFlagged,
                claim.FraudRiskLevel,
                fraudIndicators,
                claim.RequiresHumanReview,
                claim.FinalDecision,
                claim.ReviewerComments,
                claim.RejectionReason,
                claim.ReviewedAt,
                reviewedByUser != null ? $"{reviewedByUser.Username} {reviewedByUser.Email}" : null,
                claim.IsResubmitted,
                claim.ResubmittedAt,
                ParseJsonSafely(claim.CIMASRequestPayload),
                ParseJsonSafely(claim.CIMASResponsePayload),
                ParseJsonSafely(claim.AgentAnalysisPayload)
            );

            return Result.Ok(claimDetail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting claim {ClaimId}", claimId);
            return Result.Exception("Error retrieving claim");
        }
    }

    public async Task<Result<GetClaimsResponse>> GetClaims(GetClaimsRequest request)
    {
        try
        {
            var query = _repository.Claims.AsQueryable();

            // Apply filters
            if (request.Status.HasValue)
                query = query.Where(c => c.Status == request.Status.Value);

            if (request.Source.HasValue)
                query = query.Where(c => c.Source == request.Source.Value);

            if (request.RequiresReview.HasValue)
                query = query.Where(c => c.RequiresHumanReview == request.RequiresReview.Value);

            if (request.IsFlagged.HasValue)
                query = query.Where(c => c.IsFlagged == request.IsFlagged.Value);

            if (request.FromDate.HasValue)
                query = query.Where(c => c.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(c => c.CreatedAt <= request.ToDate.Value);

            if (!string.IsNullOrEmpty(request.PatientName))
                query = query.Where(c => c.PatientName.Contains(request.PatientName));

            if (!string.IsNullOrEmpty(request.ClaimNumber))
                query = query.Where(c => c.ClaimNumber.Contains(request.ClaimNumber));

            var totalCount = await query.CountAsync();

            var claims = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new ClaimSummaryDto(
                    c.Id,
                    c.ClaimNumber,
                    c.PatientName,
                    c.ProviderPracticeName,
                    c.ClaimAmount,
                    c.ApprovedAmount,
                    c.Status,
                    c.FraudRiskLevel,
                    c.RequiresHumanReview,
                    c.CreatedAt,
                    c.ReviewedAt,
                    c.AgentRecommendation
                ))
                .ToArrayAsync();

            // Calculate stats
            var stats = await CalculateClaimStats();

            return Result.Ok(new GetClaimsResponse(claims, totalCount, stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting claims");
            return Result.Exception("Error retrieving claims");
        }
    }

    public async Task<Result<ClaimModel>> UpdateClaimStatus(Guid claimId, UpdateClaimStatusRequest request, Guid userId)
    {
        try
        {
            var claim = await _repository.Claims.FindAsync(claimId);
            if (claim == null)
            {
                return Result.Fail("Claim not found");
            }

            var oldStatus = claim.Status;
            claim.Status = request.Status;
            claim.UpdatedAt = DateTime.UtcNow;

            // Add audit log entry
            await AddAuditLogEntry(claim, "StatusChanged", userId, $"Status changed from {oldStatus} to {request.Status}. Comments: {request.Comments}");

            await _repository.SaveChangesAsync();

            _logger.LogInformation("Updated claim {ClaimId} status from {OldStatus} to {NewStatus}", claimId, oldStatus, request.Status);

            return Result.Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating claim status");
            return Result.Exception("Error updating claim status");
        }
    }

    public async Task<Result<ClaimModel>> ReviewClaim(Guid claimId, ReviewClaimRequest request, Guid userId)
    {
        try
        {
            var claim = await _repository.Claims.FindAsync(claimId);
            if (claim == null)
            {
                return Result.Fail("Claim not found");
            }

            claim.FinalDecision = request.Decision;
            claim.ReviewerComments = request.ReviewerComments;
            claim.RejectionReason = request.RejectionReason;
            claim.ReviewedAt = DateTime.UtcNow;
            claim.ReviewedById = userId;
            claim.UpdatedAt = DateTime.UtcNow;

            // Update status based on decision
            claim.Status = request.Decision switch
            {
                ClaimDecision.Approve => ClaimStatus.Approved,
                ClaimDecision.Reject => ClaimStatus.Rejected,
                ClaimDecision.RequestMoreInfo => ClaimStatus.RequestMoreInfo,
                ClaimDecision.Escalate => ClaimStatus.PendingReview,
                _ => claim.Status
            };

            await AddAuditLogEntry(claim, "Reviewed", userId, $"Claim reviewed with decision: {request.Decision}");

            await _repository.SaveChangesAsync();

            _logger.LogInformation("Reviewed claim {ClaimId} with decision {Decision}", claimId, request.Decision);

            return Result.Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing claim");
            return Result.Exception("Error reviewing claim");
        }
    }

    public async Task<Result<ClaimModel>> ProcessClaim(Guid claimId, ProcessClaimRequest request)
    {
        try
        {
            var claim = await _repository.Claims.FindAsync(claimId);
            if (claim == null)
            {
                return Result.Fail("Claim not found");
            }

            claim.AgentRecommendation = request.AgentRecommendation;
            claim.AgentReasoning = request.AgentReasoning;
            claim.AgentConfidenceScore = request.AgentConfidenceScore;
            claim.AgentAnalysisPayload = request.AgentAnalysisPayload;
            claim.AgentProcessedAt = DateTime.UtcNow;
            claim.Status = ClaimStatus.PendingReview;
            claim.UpdatedAt = DateTime.UtcNow;

            await AddAuditLogEntry(claim, "AgentProcessed", Guid.Empty, $"AI Agent processed claim with recommendation: {request.AgentRecommendation}");

            await _repository.SaveChangesAsync();

            _logger.LogInformation("AI processed claim {ClaimId} with recommendation {Recommendation}", claimId, request.AgentRecommendation);

            return Result.Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing claim with AI");
            return Result.Exception("Error processing claim");
        }
    }

    public async Task<Result<ClaimModel[]>> BatchReviewClaims(BatchReviewClaimsRequest request, Guid userId)
    {
        try
        {
            var claims = await _repository.Claims
                .Where(c => request.ClaimIds.Contains(c.Id))
                .ToArrayAsync();

            if (claims.Length != request.ClaimIds.Length)
            {
                return Result.Fail("Some claims were not found");
            }

            foreach (var claim in claims)
            {
                claim.FinalDecision = request.Decision;
                claim.ReviewerComments = request.Comments;
                claim.ReviewedAt = DateTime.UtcNow;
                claim.ReviewedById = userId;
                claim.UpdatedAt = DateTime.UtcNow;

                claim.Status = request.Decision switch
                {
                    ClaimDecision.Approve => ClaimStatus.Approved,
                    ClaimDecision.Reject => ClaimStatus.Rejected,
                    ClaimDecision.RequestMoreInfo => ClaimStatus.RequestMoreInfo,
                    ClaimDecision.Escalate => ClaimStatus.PendingReview,
                    _ => claim.Status
                };

                await AddAuditLogEntry(claim, "BatchReviewed", userId, $"Batch reviewed with decision: {request.Decision}");
            }

            await _repository.SaveChangesAsync();

            _logger.LogInformation("Batch reviewed {Count} claims with decision {Decision}", claims.Length, request.Decision);

            return Result.Ok(claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch reviewing claims");
            return Result.Exception("Error batch reviewing claims");
        }
    }

    public async Task<Result<ClaimModel>> ResubmitClaim(ResubmitClaimRequest request, Guid userId)
    {
        try
        {
            var claim = await _repository.Claims.FindAsync(request.ClaimId);
            if (claim == null)
            {
                return Result.Fail("Claim not found");
            }

            // TODO: Implement actual CIMAS resubmission using _cimasProvider
            // For now, just update the claim record
            
            claim.ResubmissionPayload = JsonSerializer.Serialize(request.ModifiedCIMASRequest);
            claim.IsResubmitted = true;
            claim.ResubmittedAt = DateTime.UtcNow;
            claim.Status = ClaimStatus.Resubmitted;
            claim.UpdatedAt = DateTime.UtcNow;

            await AddAuditLogEntry(claim, "Resubmitted", userId, "Claim resubmitted to CIMAS with modifications");

            await _repository.SaveChangesAsync();

            _logger.LogInformation("Resubmitted claim {ClaimId} to CIMAS", request.ClaimId);

            return Result.Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resubmitting claim");
            return Result.Exception("Error resubmitting claim");
        }
    }

    private async Task<ClaimStatsDto> CalculateClaimStats()
    {
        var totalClaims = await _repository.Claims.CountAsync();
        var pendingClaims = await _repository.Claims.CountAsync(c => c.Status == ClaimStatus.Pending);
        var pendingReviewClaims = await _repository.Claims.CountAsync(c => c.Status == ClaimStatus.PendingReview);
        var approvedClaims = await _repository.Claims.CountAsync(c => c.Status == ClaimStatus.Approved);
        var rejectedClaims = await _repository.Claims.CountAsync(c => c.Status == ClaimStatus.Rejected);
        var flaggedClaims = await _repository.Claims.CountAsync(c => c.IsFlagged);

        var totalClaimAmount = await _repository.Claims.SumAsync(c => c.ClaimAmount);
        var totalApprovedAmount = await _repository.Claims.SumAsync(c => c.ApprovedAmount);

        // Calculate average processing time for completed claims
        var completedClaims = await _repository.Claims
            .Where(c => c.ReviewedAt.HasValue && c.CreatedAt != null)
            .Select(c => new { c.CreatedAt, c.ReviewedAt })
            .ToListAsync();

        var averageProcessingTime = completedClaims.Any()
            ? completedClaims.Average(c => (c.ReviewedAt!.Value - c.CreatedAt).TotalHours)
            : 0;

        return new ClaimStatsDto(
            totalClaims,
            pendingClaims,
            pendingReviewClaims,
            approvedClaims,
            rejectedClaims,
            flaggedClaims,
            totalClaimAmount,
            totalApprovedAmount,
            averageProcessingTime
        );
    }

    private async Task AddAuditLogEntry(ClaimModel claim, string action, Guid userId, string details)
    {
        var existingLog = string.IsNullOrEmpty(claim.AuditLog) 
            ? new List<object>()
            : JsonSerializer.Deserialize<List<object>>(claim.AuditLog) ?? new List<object>();

        existingLog.Add(new 
        { 
            Action = action, 
            Timestamp = DateTime.UtcNow, 
            UserId = userId, 
            Details = details 
        });

        claim.AuditLog = JsonSerializer.Serialize(existingLog);
    }

    private static object? ParseJsonSafely(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<object>(json);
        }
        catch
        {
            return null;
        }
    }
} 