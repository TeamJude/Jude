using System.Threading.Tasks;
using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.KernelMemory;

namespace Jude.Server.Domains.Policies;

public interface IPolicyService
{
    Task<Result<PolicyModel>> UploadPolicyAsync(IFormFile file, string name, Guid createdById);
    Task<Result<GetPoliciesResponse>> GetPolicies(GetPoliciesRequest request);
}

public class PolicyService : IPolicyService
{
    private readonly JudeDbContext _dbContext;
    private readonly IPolicyContext _policyContext;

    public PolicyService(JudeDbContext dbContext, IPolicyContext policyContext)
    {
        _dbContext = dbContext;
        _policyContext = policyContext;
    }

    public async Task<Result<PolicyModel>> UploadPolicyAsync(
        IFormFile file,
        string name,
        Guid createdById
    )
    {
        if (file == null || file.Length == 0)
            return Result.Fail("File is required.");
        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail("Policy name is required.");

        using var stream = file.OpenReadStream();

        var ingestResult = await _policyContext.Ingest(
            stream,
            new TagCollection { { "name", name } }
        );

        if (!ingestResult.Success)
            return Result.Fail(
                ingestResult.Errors.FirstOrDefault() ?? "Failed to ingest document."
            );

        var policy = new PolicyModel
        {
            Name = name,
            DocumentUrl = "C:\\Users\\devma\\Desktop\\policies\\policy1.pdf",
            DocumentId = ingestResult.Data,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = PolicyStatus.Active,
        };

        _dbContext.Policies.Add(policy);
        await _dbContext.SaveChangesAsync();
        return Result.Ok(policy);
    }

    public async Task<Result<GetPoliciesResponse>> GetPolicies(GetPoliciesRequest request)
    {
        var query = _dbContext.Policies.AsQueryable();

        var totalCount = await query.CountAsync();
        var policies = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PolicyResponse(
                p.Id,
                p.Name,
                p.DocumentId,
                p.DocumentUrl,
                p.Status,
                p.CreatedAt,
                p.UpdatedAt,
                p.CreatedById
            ))
            .ToArrayAsync();

        return Result.Ok(new GetPoliciesResponse(policies, totalCount));
    }
}
