using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Jude.Server.Config;
using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Policies.Events;
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
    private readonly ILogger<PolicyService> _logger;
    private readonly JudeDbContext _dbContext;
    private readonly IPolicyContext _policyContext;
    private readonly IPolicyIngestEventsQueue _ingestQueue;

    private readonly BlobContainerClient _blobContainerClient;

    public PolicyService(
        JudeDbContext dbContext,
        IPolicyContext policyContext,
        ILogger<PolicyService> logger,
        IPolicyIngestEventsQueue ingestQueue
    )
    {
        _logger = logger;
        _dbContext = dbContext;
        _policyContext = policyContext;
        _ingestQueue = ingestQueue;
        _blobContainerClient = new BlobServiceClient(
            AppConfig.Azure.Blob.ConnectionString
        ).GetBlobContainerClient(AppConfig.Azure.Blob.Container);
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

        _logger.LogInformation(
            "Starting policy upload for {PolicyName} by user {CreatedById}",
            name,
            createdById
        );

        var policy = new PolicyModel
        {
            Name = name,
            DocumentId = null,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = PolicyStatus.Pending,
        };

        _dbContext.Policies.Add(policy);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Created policy {PolicyId} - {PolicyName} in pending state",
            policy.Id,
            policy.Name
        );

        // Read file content into byte array to avoid disposed stream issues
        byte[] fileContent;
        using (var stream = file.OpenReadStream())
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                fileContent = memoryStream.ToArray();
            }
        }

        var ingestEvent = new PolicyIngestEvent(
            PolicyId: policy.Id,
            PolicyName: policy.Name,
            FileContent: fileContent,
            FileName: file.FileName,
            IngestedAt: DateTime.UtcNow
        );

        await _ingestQueue.Writer.WriteAsync(ingestEvent);

        _logger.LogInformation(
            "Enqueued policy {PolicyId} - {PolicyName} for background ingest processing",
            policy.Id,
            policy.Name
        );

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
                p.DocumentId ?? "",
                p.DocumentUrl ?? "",
                p.Status,
                p.CreatedAt,
                p.UpdatedAt,
                p.CreatedById
            ))
            .ToArrayAsync();

        return Result.Ok(new GetPoliciesResponse(policies, totalCount));
    }
}
