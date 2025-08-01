using System.Threading.Channels;

namespace Jude.Server.Domains.Policies.Events;

public interface IPolicyIngestEventsQueue
{
    ChannelReader<PolicyIngestEvent> Reader { get; }
    ChannelWriter<PolicyIngestEvent> Writer { get; }
}

public class PolicyIngestEventsQueue : IPolicyIngestEventsQueue
{
    private readonly Channel<PolicyIngestEvent> _channel;

    public PolicyIngestEventsQueue()
    {
        _channel = Channel.CreateUnbounded<PolicyIngestEvent>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
        );
    }

    public ChannelReader<PolicyIngestEvent> Reader => _channel.Reader;
    public ChannelWriter<PolicyIngestEvent> Writer => _channel.Writer;
}
