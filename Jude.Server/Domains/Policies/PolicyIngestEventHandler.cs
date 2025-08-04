using Jude.Server.Config;
using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Policies.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.KernelMemory;

namespace Jude.Server.Domains.Policies;

public interface IPolicyIngestEventHandler
{
    Task HandlePolicyIngestAsync(PolicyIngestEvent ingestEvent);
}

public class PolicyIngestEventHandler : IPolicyIngestEventHandler
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<PolicyIngestEventHandler> _logger;

    public PolicyIngestEventHandler(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<PolicyIngestEventHandler> logger
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task HandlePolicyIngestAsync(PolicyIngestEvent ingestEvent)
    {
        _logger.LogInformation(
            "Starting policy ingest for policy {PolicyId} - {PolicyName}",
            ingestEvent.PolicyId,
            ingestEvent.PolicyName
        );

        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JudeDbContext>();
        var policyContext = scope.ServiceProvider.GetRequiredService<IPolicyContext>();

        var policy = await dbContext.Policies.FirstOrDefaultAsync(p =>
            p.Id == ingestEvent.PolicyId
        );

        if (policy == null)
        {
            _logger.LogError("Policy with ID {PolicyId} not found", ingestEvent.PolicyId);
            return;
        }

        var fileName =
            ingestEvent.PolicyName
            + "_"
            + Guid.NewGuid().ToString()
            + Path.GetExtension(ingestEvent.FileName);

        using var stream = new MemoryStream(ingestEvent.FileContent);
        var ingestResult = await policyContext.Ingest(
            stream,
            fileName,
            new TagCollection { { "name", ingestEvent.PolicyName } }
        );

        if (ingestResult.Success)
        {
            policy.DocumentUrl = CreateDocumentUrl(
                fileName,
                AppConfig.PolicyIndexName,
                ingestResult.Data!
            );

            policy.DocumentId = ingestResult.Data;
            policy.Status = PolicyStatus.Active;
            policy.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Successfully ingested policy {PolicyId} - {PolicyName} with DocumentId {DocumentId}",
                ingestEvent.PolicyId,
                ingestEvent.PolicyName,
                ingestResult.Data
            );
        }
        else
        {
            policy.Status = PolicyStatus.Failed;
            policy.UpdatedAt = DateTime.UtcNow;

            _logger.LogError(
                "Failed to ingest policy {PolicyId} - {PolicyName}: {Errors}",
                ingestEvent.PolicyId,
                ingestEvent.PolicyName,
                string.Join(", ", ingestResult.Errors)
            );
        }

        await dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Completed policy ingest processing for policy {PolicyId} with status {Status}",
            ingestEvent.PolicyId,
            policy.Status
        );
    }

    private static string CreateDocumentUrl(string Filename, string index, string documentId)
    {
        return $"{AppConfig.Azure.Blob.BaseUrl}/{AppConfig.Azure.Blob.Container}/{index}/{documentId}/{Filename}";
    }
}
