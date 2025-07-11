using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Domains.Fraud;

public interface IFraudService
{
    public Task<Result<FraudIndicatorResponse>> CreateFraudIndicator(
        CreateFraudIndicatorRequest request,
        Guid userId
    );
    public Task<Result<GetFraudIndicatorsResponse>> GetFraudIndicators(
        GetFraudIndicatorsRequest request
    );
    public Task<FraudIndicatorResponse[]> GetActiveFraudIndicators();
    public event Action? OnFraudIndicatorsChanged;
}

public class FraudService : IFraudService
{
    private readonly JudeDbContext _repository;

    public FraudService(JudeDbContext repository)
    {
        _repository = repository;
    }

    public event Action? OnFraudIndicatorsChanged;

    public async Task<Result<FraudIndicatorResponse>> CreateFraudIndicator(
        CreateFraudIndicatorRequest request,
        Guid userId
    )
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

        var response = new FraudIndicatorResponse(
            fraudIndicator.Id,
            fraudIndicator.CreatedAt,
            fraudIndicator.Name,
            fraudIndicator.Description,
            fraudIndicator.Status,
            fraudIndicator.CreatedById
        );

        // Notify that fraud indicators have changed
        OnFraudIndicatorsChanged?.Invoke();

        return Result.Ok(response);
    }

    public async Task<Result<GetFraudIndicatorsResponse>> GetFraudIndicators(
        GetFraudIndicatorsRequest request
    )
    {
        var query = _repository.FraudIndicators.AsQueryable();

        var totalCount = await query.CountAsync();
        var fraudIndicators = await query
            .OrderBy(f => f.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(f => new FraudIndicatorResponse(
                f.Id,
                f.CreatedAt,
                f.Name,
                f.Description,
                f.Status,
                f.CreatedById
            ))
            .ToArrayAsync();

        return Result.Ok(new GetFraudIndicatorsResponse(fraudIndicators, totalCount));
    }

    public async Task<FraudIndicatorResponse[]> GetActiveFraudIndicators()
    {
        var activeIndicators = await _repository.FraudIndicators
            .Where(f => f.Status == IndicatorStatus.Active)
            .Select(f => new FraudIndicatorResponse(
                f.Id,
                f.CreatedAt,
                f.Name,
                f.Description,
                f.Status,
                f.CreatedById
            ))
            .ToArrayAsync();

        return activeIndicators;
    }
}
