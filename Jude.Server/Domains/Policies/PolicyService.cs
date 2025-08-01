using System.Threading.Tasks;
using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.KernelMemory;
using Azure.Storage.Blobs;
using Jude.Server.Config;
using Jude.Server.Domains.Policies.Events;

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
        IPolicyIngestEventsQueue ingestQueue)
    {
        _logger = logger;
        _dbContext = dbContext;
        _policyContext = policyContext;
        _ingestQueue = ingestQueue;
        _blobContainerClient = new BlobServiceClient(AppConfig.Azure.Blob.ConnectionString)
            .GetBlobContainerClient(AppConfig.Azure.Blob.Container);
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

        try
        {
            byte[] fileContent;
            using (var stream = file.OpenReadStream())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    fileContent = memoryStream.ToArray();
                }
            }

            using (var blobStream = new MemoryStream(fileContent))
            {
                var uploadToBlobStorageResult = await UploadToBlobStorageAsync(
                    blobStream,
                    $"{Guid.NewGuid()}_{file.FileName}"
                );

                if (!uploadToBlobStorageResult.Success)
                {
                    _logger.LogError(
                        "Failed to upload policy {PolicyName} to blob storage: {Errors}",
                        name,
                        string.Join(", ", uploadToBlobStorageResult.Errors)
                    );
                    return Result.Fail(uploadToBlobStorageResult.Errors.FirstOrDefault() ?? "Failed to upload document.");
                }

                var policy = new PolicyModel
                {
                    Name = name,
                    DocumentUrl = uploadToBlobStorageResult.Data,
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error uploading policy {PolicyName}: {Message}",
                name,
                ex.Message
            );
            return Result.Fail($"Failed to upload policy: {ex.Message}");
        }
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

    async Task<Result<string>> UploadToBlobStorageAsync(
        Stream stream,
        string fileName
    )
    {
        if (stream == null || stream.Length == 0)
            return Result.Fail("Stream is required.");

        fileName = $"{Guid.NewGuid()}_{fileName}";
        var blobClient = _blobContainerClient.GetBlobClient(fileName);
        try
        {
            await blobClient.UploadAsync(stream, overwrite: true);
            return Result.Ok(blobClient.Uri.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file to blob storage.");
            return Result.Fail($"Failed to upload file.");
        }
    }
}
