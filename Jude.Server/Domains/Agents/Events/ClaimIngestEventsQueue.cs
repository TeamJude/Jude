using System.Threading.Channels;

namespace Jude.Server.Domains.Agents.Events;

public interface IClaimIngestEventsQueue
{
    ChannelReader<ClaimIngestEvent> Reader { get; }
    ChannelWriter<ClaimIngestEvent> Writer { get; }
}

public class ClaimIngestEventsQueue : IClaimIngestEventsQueue
{
    private readonly Channel<ClaimIngestEvent> _channel;

    public ClaimIngestEventsQueue()
    {
        _channel = Channel.CreateUnbounded<ClaimIngestEvent>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
        );
    }

    public ChannelReader<ClaimIngestEvent> Reader => _channel.Reader;
    public ChannelWriter<ClaimIngestEvent> Writer => _channel.Writer;
}
