using System.Threading.Channels;

namespace Jude.Server.Domains.Agents.Events;

public interface IClaimBulkInsertEventsQueue
{
    ChannelReader<ClaimBulkInsertEvent> Reader { get; }
    ChannelWriter<ClaimBulkInsertEvent> Writer { get; }
}

public class ClaimBulkInsertEventsQueue : IClaimBulkInsertEventsQueue
{
    private readonly Channel<ClaimBulkInsertEvent> _channel;

    public ClaimBulkInsertEventsQueue()
    {
        _channel = Channel.CreateUnbounded<ClaimBulkInsertEvent>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
        );
    }

    public ChannelReader<ClaimBulkInsertEvent> Reader => _channel.Reader;
    public ChannelWriter<ClaimBulkInsertEvent> Writer => _channel.Writer;
}

