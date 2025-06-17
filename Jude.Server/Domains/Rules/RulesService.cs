using System.Data;
using System.Runtime.InteropServices;
using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Rules;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Domains.Rules;

public interface IRulesService
{
    public Task<Result<RuleModel>> CreateRule(CreateRuleRequest request, Guid userId);
    public Task<Result<GetRulesResponse>> GetRules(GetRulesRequest request);
}

public class RulesService : IRulesService
{
    private readonly JudeDbContext _repository;

    public RulesService(JudeDbContext repository)
    {
        _repository = repository;
    }

    public async Task<Result<RuleModel>> CreateRule(CreateRuleRequest request, Guid userId)
    {
        var rule = new RuleModel
        {
            Name = request.Name,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            CreatedById = userId
        };

        await _repository.Rules.AddAsync(rule);
        await _repository.SaveChangesAsync();

        return Result.Ok(rule);
    }

    public async Task<Result<GetRulesResponse>> GetRules(GetRulesRequest request)
    {
        var query = _repository.Rules.AsQueryable();

        var totalCount = await query.CountAsync();
        var rules = await query
            .OrderBy(r => r.Priority)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToArrayAsync();

        return Result.Ok(new GetRulesResponse(rules, totalCount));
    }
}