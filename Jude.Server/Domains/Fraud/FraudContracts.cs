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

public record GetFraudIndicatorsResponse(FraudIndicatorModel[] FraudIndicators, int TotalCount); 