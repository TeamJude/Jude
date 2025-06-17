using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Domains.Fraud;

public interface IFraudService
{
    public Task<Result<FraudIndicatorModel>> CreateFraudIndicator(CreateFraudIndicatorRequest request, Guid userId);
    public Task<Result<GetFraudIndicatorsResponse>> GetFraudIndicators(GetFraudIndicatorsRequest request);
}

public class FraudService : IFraudService
{
    private readonly JudeDbContext _repository;

    public FraudService(JudeDbContext repository)
    {
        _repository = repository;
    }

    public async Task<Result<FraudIndicatorModel>> CreateFraudIndicator(CreateFraudIndicatorRequest request, Guid userId)
    {
        var fraudIndicator = new FraudIndicatorModel
        {
            Name = request.Name,
            Description = request.Description,
            Status = request.Status,
            CreatedById = userId,
        };

        await _repository.FraudIndicators.AddAsync(fraudIndicator);
        await _repository.SaveChangesAsync();

        return Result.Ok(fraudIndicator);
    }

    public async Task<Result<GetFraudIndicatorsResponse>> GetFraudIndicators(GetFraudIndicatorsRequest request)
    {
        var query = _repository.FraudIndicators.AsQueryable();

        var totalCount = await query.CountAsync();
        var fraudIndicators = await query
            .OrderBy(f => f.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToArrayAsync();

        return Result.Ok(new GetFraudIndicatorsResponse(fraudIndicators, totalCount));
    }
} 