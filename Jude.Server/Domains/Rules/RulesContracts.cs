using Jude.Server.Data.Models;

namespace Jude.Server.Domains.Rules;

public record CreateRuleRequest(
    string Name,
    string Description,
    RuleStatus Status = RuleStatus.Active,
    int Priority = 1
);

public record GetRulesRequest(int Page = 1, int PageSize = 10);

public record GetRulesResponse(RuleResponse[] Rules, int TotalCount);

public record GetActiveRulesResponse(RuleResponse[] Rules, int TotalCount);

public record RuleResponse(
    Guid Id,
    DateTime CreatedAt,
    string Name,
    string Description,
    RuleStatus Status,
    Guid CreatedById
);
