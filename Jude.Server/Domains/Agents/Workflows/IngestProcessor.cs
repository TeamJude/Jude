using Jude.Server.Config;
using Jude.Server.Domains.Agents.Events;
using Jude.Server.Domains.Claims;

namespace Jude.Server.Domains.Agents.Workflows;

public class ClaimsIngestProcessor : BackgroundService
{
    private readonly IClaimIngestEventsQueue _queue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ClaimsIngestProcessor> _logger;

    private readonly TimeSpan _pollingInterval = TimeSpan.FromMinutes(5); // Poll every 5 minutes
    private readonly TimeSpan _initialDelay = TimeSpan.FromSeconds(30); // Wait 30 seconds before first poll

    public ClaimsIngestProcessor(
        IClaimIngestEventsQueue queue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ClaimsIngestProcessor> logger
    )
    {
        _queue = queue;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ClaimIngestProcessor starting up");

        // Wait for initial delay to ensure application is fully started
        await Task.Delay(_initialDelay, stoppingToken);

        // Start both the polling task and the queue processing task
        var pollingTask = StartPollingTask(stoppingToken);
        var processingTask = StartQueueProcessingTask(stoppingToken);

        // Wait for either task to complete (or cancellation)
        await Task.WhenAny(pollingTask, processingTask);

        _logger.LogInformation("ClaimIngestProcessor shutting down");
    }

    private async Task StartPollingTask(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Starting CIMAS polling task with interval: {Interval}",
            _pollingInterval
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollCIMASForClaims();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during CIMAS polling: {Message}", ex.Message);
            }

            // Wait for the next polling interval
            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task StartQueueProcessingTask(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting queue processing task");

        await foreach (var ingestEvent in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogDebug(
                    "Processing ingest event for transaction {TransactionNumber}",
                    ingestEvent.TransactionNumber
                );

                using var scope = _serviceScopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IClaimIngestEventHandler>();
                await handler.HandleClaimIngestAsync(ingestEvent);

                _logger.LogDebug(
                    "Successfully processed ingest event for transaction {TransactionNumber}",
                    ingestEvent.TransactionNumber
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing ingest event for transaction {TransactionNumber}: {Message}",
                    ingestEvent.TransactionNumber,
                    ex.Message
                );
            }
        }
    }

    private async Task PollCIMASForClaims()
    {
        _logger.LogDebug("Polling CIMAS for new claims");

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var claimsService = scope.ServiceProvider.GetRequiredService<IClaimsService>();

            var result = await claimsService.GetPastClaimsAsync(AppConfig.CIMAS.PracticeNumber);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "Failed to get past claims for practice {PracticeNumber}: {Errors}",
                    AppConfig.CIMAS.PracticeNumber,
                    string.Join(", ", result.Errors)
                );
            }

            var claims = result.Data;
            _logger.LogInformation(
                "Retrieved {ClaimCount} claims for practice {PracticeNumber}",
                claims.Count,
                AppConfig.CIMAS.PracticeNumber
            );

            // Process each claim
            foreach (var claim in claims)
            {
                await EnqueueClaimForProcessing(claim);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during CIMAS polling: {Message}", ex.Message);
            throw;
        }
    }

    private async Task EnqueueClaimForProcessing(Domains.Claims.Providers.CIMAS.ClaimResponse claim)
    {
        try
        {
            var transactionNumber = claim.TransactionResponse?.Number ?? "Unknown";

            _logger.LogDebug(
                "Enqueueing claim for processing: {TransactionNumber}",
                transactionNumber
            );

            var ingestEvent = new ClaimIngestEvent(
                TransactionNumber: transactionNumber,
                CIMASClaimData: claim,
                IngestedAt: DateTime.UtcNow
            );

            // Add to the processing queue
            await _queue.Writer.WriteAsync(ingestEvent);

            _logger.LogDebug("Successfully enqueued claim: {TransactionNumber}", transactionNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueueing claim for processing: {Message}", ex.Message);
        }
    }
}
