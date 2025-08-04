using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Domains.Audit;

public interface IAuditService
{
    Task<Result<bool>> AddAuditLogAsync(CreateAuditLogRequest request);
    Task<Result<GetAuditLogsResponse>> GetAuditLogsAsync(GetAuditLogsRequest request);
}

public class AuditService : IAuditService
{
    private readonly JudeDbContext _dbContext;
    private readonly ILogger<AuditService> _logger;

    public AuditService(JudeDbContext dbContext, ILogger<AuditService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<bool>> AddAuditLogAsync(CreateAuditLogRequest request)
    {
        try
        {
            var auditLog = new AuditLogModel
            {
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                Action = request.Action,
                ActorType = request.ActorType,
                ActorId = request.ActorId,
                Description = request.Description,
                Metadata = request.Metadata
            };

            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Audit log created for {EntityType} {EntityId} with action {Action}",
                request.EntityType, request.EntityId, request.Action);

            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for {EntityType} {EntityId}",
                request.EntityType, request.EntityId);
            return Result.Fail($"Failed to create audit log: {ex.Message}");
        }
    }

    public async Task<Result<GetAuditLogsResponse>> GetAuditLogsAsync(GetAuditLogsRequest request)
    {
        try
        {
            var query = _dbContext.AuditLogs.AsQueryable();

            // Apply filters
            if (request.EntityType.HasValue)
            {
                query = query.Where(a => a.EntityType == request.EntityType.Value);
            }

            if (request.EntityId.HasValue)
            {
                query = query.Where(a => a.EntityId == request.EntityId.Value);
            }

            if (request.ActorType.HasValue)
            {
                query = query.Where(a => a.ActorType == request.ActorType.Value);
            }

            if (request.ActorId.HasValue)
            {
                query = query.Where(a => a.ActorId == request.ActorId.Value);
            }

            if (!string.IsNullOrEmpty(request.Action))
            {
                query = query.Where(a => a.Action.Contains(request.Action));
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= request.ToDate.Value);
            }

            var totalCount = await query.CountAsync();

            var auditLogs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AuditLogResponse(
                    a.Id,
                    a.Timestamp,
                    a.EntityType,
                    a.EntityId,
                    a.Action,
                    a.ActorType,
                    a.ActorId,
                    a.Description,
                    a.Metadata
                ))
                .ToArrayAsync();

            var response = new GetAuditLogsResponse(auditLogs, totalCount);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return Result.Fail($"Failed to retrieve audit logs: {ex.Message}");
        }
    }
}
