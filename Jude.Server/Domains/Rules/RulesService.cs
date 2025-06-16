using System.Runtime.InteropServices;
using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Rules;

namespace Jude.Server.Domains.Rules;

public interface IRulesService
{
    public Task<Result<RuleModel>> CreateRule(CreateRuleRequest request, Guid userId);
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
            CreatedById = userId
        };

        await _repository.Rules.AddAsync(rule);
        await _repository.SaveChangesAsync();

        return Result.Ok(rule);
    }
}