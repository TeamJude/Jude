using System.Data;
using System.Runtime.InteropServices;
using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.User;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Domains.Rules;

public interface IRulesService
{
    public Task<Result<RuleResponse>> CreateRule(CreateRuleRequest request, Guid userId);
    public Task<Result<GetRulesResponse>> GetRules(GetRulesRequest request);
    public Task<RuleResponse[]> GetActiveRules();
    public event Action? OnRulesChanged;
}

public class RulesService : IRulesService
{
    private readonly JudeDbContext _repository;

    public RulesService(JudeDbContext repository)
    {
        _repository = repository;
    }

    public event Action? OnRulesChanged;

    public async Task<Result<RuleResponse>> CreateRule(CreateRuleRequest request, Guid userId)
    {
        var rule = new RuleModel
        {
            Name = request.Name,
            Description = request.Description,
            Status = request.Status,
            CreatedById = userId,
        };

        await _repository.Rules.AddAsync(rule);
        await _repository.SaveChangesAsync();

        var response = new RuleResponse(rule.Id, rule.CreatedAt, rule.Name, rule.Description, rule.Status, rule.CreatedById);

        // Notify that rules have changed
        OnRulesChanged?.Invoke();

        return Result.Ok(response);
    }

    public async Task<Result<GetRulesResponse>> GetRules(GetRulesRequest request)
    {
        var query = _repository.Rules.AsQueryable();

        var totalCount = await query.CountAsync();
        var rules = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new RuleResponse(r.Id, r.CreatedAt, r.Name, r.Description, r.Status, r.CreatedById))
            .ToArrayAsync();

        return Result.Ok(new GetRulesResponse(rules, totalCount));
    }

    public async Task<RuleResponse[]> GetActiveRules()
    {
        var activeRules = await _repository.Rules
            .Where(r => r.Status == RuleStatus.Active)
            .Select(r => new RuleResponse(r.Id, r.CreatedAt, r.Name, r.Description, r.Status, r.CreatedById))
            .ToArrayAsync();

        return activeRules;
    }
}
