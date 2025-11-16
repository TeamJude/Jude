using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Agents.Events;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Domains.Agents.Workflows;

public class ClaimsIngestProcessor : BackgroundService
{
    private readonly IClaimBulkInsertEventsQueue _bulkInsertQueue;
    private readonly IClaimIngestEventsQueue _claimQueue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ClaimsIngestProcessor> _logger;
    private readonly TimeSpan _initialDelay = TimeSpan.FromSeconds(30);

    public ClaimsIngestProcessor(
        IClaimBulkInsertEventsQueue bulkInsertQueue,
        IClaimIngestEventsQueue claimQueue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ClaimsIngestProcessor> logger
    )
    {
        _bulkInsertQueue = bulkInsertQueue;
        _claimQueue = claimQueue;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ClaimIngestProcessor starting up");

        await Task.Delay(_initialDelay, stoppingToken);

        var bulkInsertTask = ProcessBulkInsertQueue(stoppingToken);
        var claimProcessingTask = ProcessClaimQueue(stoppingToken);

        await Task.WhenAny(bulkInsertTask, claimProcessingTask);

        _logger.LogInformation("ClaimIngestProcessor shutting down");
    }

    private async Task ProcessBulkInsertQueue(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting bulk insert queue processing");

        await foreach (
            var bulkInsertEvent in _bulkInsertQueue.Reader.ReadAllAsync(stoppingToken)
        )
        {
            try
            {
                _logger.LogInformation(
                    "Processing bulk insert event for {Count} claims from file {FileName}",
                    bulkInsertEvent.Claims.Count,
                    bulkInsertEvent.FileName
                );

                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<JudeDbContext>();

                await BulkInsertAndQueueClaimsAsync(
                    bulkInsertEvent.Claims,
                    bulkInsertEvent.FileName,
                    dbContext
                );

                _logger.LogInformation(
                    "Successfully processed bulk insert for file {FileName}",
                    bulkInsertEvent.FileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing bulk insert event for file {FileName}",
                    bulkInsertEvent.FileName
                );
            }
        }
    }

    private async Task ProcessClaimQueue(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting claim processing queue");

        await foreach (var ingestEvent in _claimQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogDebug(
                    "Processing ingest event for claim {ClaimID}",
                    ingestEvent.Claim.Id
                );

                using var scope = _serviceScopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IClaimIngestEventHandler>();
                await handler.HandleClaimIngestAsync(ingestEvent);

                _logger.LogDebug(
                    "Successfully processed ingest event for claim {ClaimId}",
                    ingestEvent.Claim.Id
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing ingest event for claim {ClaimId}: {Message}",
                    ingestEvent.Claim.Id,
                    ex.Message
                );
            }
        }
    }

    private async Task BulkInsertAndQueueClaimsAsync(
        List<ClaimModel> claims,
        string fileName,
        JudeDbContext dbContext
    )
    {
        var insertedCount = 0;
        var duplicateCount = 0;
        var queuedCount = 0;
        var failedCount = 0;

        _logger.LogInformation(
            "Starting bulk insert of {Count} claims from file {FileName}",
            claims.Count,
            fileName
        );

        dbContext.Claims.AddRange(claims);

        try
        {
            await dbContext.SaveChangesAsync();
            insertedCount = claims.Count;

            _logger.LogInformation(
                "Successfully bulk inserted all {Count} claims",
                claims.Count
            );
        }
        catch (DbUpdateException ex)
            when (ex.InnerException?.Message.Contains("duplicate key") == true
                || ex.InnerException?.Message.Contains("unique constraint") == true
            )
        {
            _logger.LogInformation(
                "Bulk insert encountered duplicates. Falling back to individual inserts."
            );

            dbContext.ChangeTracker.Clear();

            foreach (var claim in claims)
            {
                try
                {
                    dbContext.Claims.Add(claim);
                    await dbContext.SaveChangesAsync();
                    insertedCount++;
                    _logger.LogDebug(
                        "Inserted claim {ClaimNumber} into database",
                        claim.ClaimNumber
                    );
                }
                catch (DbUpdateException dbEx)
                    when (dbEx.InnerException?.Message.Contains("duplicate key") == true
                        || dbEx.InnerException?.Message.Contains("unique constraint") == true
                    )
                {
                    duplicateCount++;
                    _logger.LogDebug(
                        "Skipped duplicate claim {ClaimNumber}",
                        claim.ClaimNumber
                    );
                    dbContext.ChangeTracker.Clear();
                }
                catch (Exception individualEx)
                {
                    failedCount++;
                    _logger.LogError(
                        individualEx,
                        "Error inserting claim {ClaimNumber}",
                        claim.ClaimNumber
                    );
                    dbContext.ChangeTracker.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk insert operation");
            failedCount = claims.Count;
        }

        _logger.LogInformation(
            "Bulk insert complete. Inserted: {Inserted}, Duplicates: {Duplicates}, Failed: {Failed}. Now queueing for processing.",
            insertedCount,
            duplicateCount,
            failedCount
        );

        var pendingClaims = await dbContext
            .Claims.Where(c =>
                c.Status == ClaimStatus.Pending
                && claims.Select(cl => cl.ClaimNumber).Contains(c.ClaimNumber)
            )
            .ToListAsync();

        foreach (var claim in pendingClaims)
        {
            var auditLog = new AuditLogModel
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                EntityType = AuditEntityType.Claim,
                EntityId = claim.Id,
                Action = "Claim Ingested",
                ActorType = AuditActorType.System,
                ActorId = null,
                Description = $"Claim {claim.ClaimNumber} was ingested from Excel file: {fileName}",
                Metadata = new Dictionary<string, object>
                {
                    { "fileName", fileName },
                    { "claimNumber", claim.ClaimNumber }
                }
            };

            await dbContext.AuditLogs.AddAsync(auditLog);

            var ingestEvent = new ClaimIngestEvent(claim, DateTime.UtcNow);
            await _claimQueue.Writer.WriteAsync(ingestEvent);
            queuedCount++;
        }

        await dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Background processing complete for file {FileName}. Total: {Total}, Inserted: {Inserted}, Duplicates: {Duplicates}, Failed: {Failed}, Queued: {Queued}",
            fileName,
            claims.Count,
            insertedCount,
            duplicateCount,
            failedCount,
            queuedCount
        );
    }
}
