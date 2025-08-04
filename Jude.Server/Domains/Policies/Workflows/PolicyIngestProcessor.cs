using Jude.Server.Domains.Policies.Events;

namespace Jude.Server.Domains.Policies.Workflows;

public class PolicyIngestProcessor : BackgroundService
{
    private readonly IPolicyIngestEventsQueue _queue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<PolicyIngestProcessor> _logger;

    public PolicyIngestProcessor(
        IPolicyIngestEventsQueue queue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<PolicyIngestProcessor> logger
    )
    {
        _queue = queue;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PolicyIngestProcessor starting up");

        await foreach (var ingestEvent in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogDebug(
                    "Processing policy ingest event for policy {PolicyId} - {PolicyName}",
                    ingestEvent.PolicyId,
                    ingestEvent.PolicyName
                );

                using var scope = _serviceScopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IPolicyIngestEventHandler>();
                await handler.HandlePolicyIngestAsync(ingestEvent);

                _logger.LogDebug(
                    "Successfully processed policy ingest event for policy {PolicyId} - {PolicyName}",
                    ingestEvent.PolicyId,
                    ingestEvent.PolicyName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing policy ingest event for policy {PolicyId} - {PolicyName}: {Message}",
                    ingestEvent.PolicyId,
                    ingestEvent.PolicyName,
                    ex.Message
                );
            }
        }

        _logger.LogInformation("PolicyIngestProcessor shutting down");
    }
}
