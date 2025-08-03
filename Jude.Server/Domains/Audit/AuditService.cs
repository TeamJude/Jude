using System;
using System.Linq;
using System.Threading.Tasks;
using Jude.Server.Core.Helpers;
using Jude.Server.Data;
using Jude.Server.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Domains.Audit;

public interface IAuditService
{
    Task<Result<AuditResponse>> CreateAuditEntryAsync(CreateAuditRequest request);
    Task<Result<GetAuditResponse>> GetAuditEntriesAsync(GetAuditRequest request);
    Task<Result<AuditResponse[]>> GetEntityAuditHistoryAsync(Guid entityId, string entityType);
}

public class AuditService : IAuditService
{
    private readonly JudeDbContext _context;

    public AuditService(JudeDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AuditResponse>> CreateAuditEntryAsync(CreateAuditRequest request)
    {
        var auditEntry = new AuditModel
        {
            Id = Guid.NewGuid(),
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Action = request.Action,
            ActorType = request.ActorType,
            ActorId = request.ActorId,
            ActorName = request.ActorName,
            Description = request.Description,
            Metadata = request.Metadata
        };

        _context.Audits.Add(auditEntry);
        await _context.SaveChangesAsync();

        return Result<AuditResponse>.Success(MapToResponse(auditEntry));
    }

    public async Task<Result<GetAuditResponse>> GetAuditEntriesAsync(GetAuditRequest request)
    {
        var query = _context.Audits.AsQueryable();

        if (request.StartDate.HasValue)
            query = query.Where(a => a.Timestamp >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(a => a.Timestamp <= request.EndDate.Value);

        if (!string.IsNullOrEmpty(request.EntityType))
            query = query.Where(a => a.EntityType == request.EntityType);

        if (!string.IsNullOrEmpty(request.Action))
            query = query.Where(a => a.Action == request.Action);

        if (!string.IsNullOrEmpty(request.ActorType))
            query = query.Where(a => a.ActorType == request.ActorType);

        var totalCount = await query.CountAsync();
        var entries = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToArrayAsync();

        return Result<GetAuditResponse>.Success(new GetAuditResponse(
            entries.Select(MapToResponse).ToArray(),
            totalCount,
            request.Page,
            request.PageSize
        ));
    }

    public async Task<Result<AuditResponse[]>> GetEntityAuditHistoryAsync(Guid entityId, string entityType)
    {
        var entries = await _context.Audits
            .Where(a => a.EntityId == entityId && a.EntityType == entityType)
            .OrderByDescending(a => a.Timestamp)
            .ToArrayAsync();

        return Result<AuditResponse[]>.Success(entries.Select(MapToResponse).ToArray());
    }

    private static AuditResponse MapToResponse(AuditModel model) => new(
        model.Id,
        model.Timestamp,
        model.EntityType,
        model.EntityId,
        model.Action,
        model.ActorType,
        model.ActorId,
        model.ActorName,
        model.Description,
        model.Metadata
    );
}
