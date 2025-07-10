using Jude.Server.Data.Models;

namespace Jude.Server.Domains.Fraud;

public record CreateFraudIndicatorRequest(
    string Name,
    string Description,
    IndicatorStatus Status = IndicatorStatus.Active
);

public record GetFraudIndicatorsRequest(
    int Page = 1,
    int PageSize = 10
// TODO: add more filters in the future
);

public record FraudIndicatorResponse(
    Guid Id,
    DateTime CreatedAt,
    string Name,
    string Description,
    IndicatorStatus Status,
    Guid CreatedById
);

public record GetFraudIndicatorsResponse(FraudIndicatorResponse[] FraudIndicators, int TotalCount);
