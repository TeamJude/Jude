using Jude.Server.Data.Models;

namespace Jude.Server.Domains.Rules;

public record CreateRuleRequest(
    string Name,
    string Description,
    RuleStatus Status = RuleStatus.Active,
    int Priority = 1
);

public record GetRulesRequest(
    int Page = 1,
    int PageSize = 10
// TODO: add more filters in the future
);

public record GetRulesResponse(RuleModel[] Rules, int TotalCount);
