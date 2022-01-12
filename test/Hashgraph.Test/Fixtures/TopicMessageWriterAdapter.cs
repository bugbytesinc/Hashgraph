using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Hashgraph.Test.Fixtures;

public sealed class TopicMessageWriterAdapter
{
    private Channel<TopicMessage> _channel;
    private readonly Task _readTask;

    public ChannelWriter<TopicMessage> MessageWriter { get { return _channel.Writer; } }
    public TopicMessageWriterAdapter(Action<TopicMessage> handle) : this(32, handle) { }
    public TopicMessageWriterAdapter(int bufferCapacity, Action<TopicMessage> handle)
    {
        _channel = Channel.CreateBounded<TopicMessage>(bufferCapacity);
        var reader = _channel.Reader;
        var writer = _channel.Writer;
        _readTask = Task.Run(async () =>
        {
            try
            {
                while (await reader.WaitToReadAsync())
                {
                    if (reader.TryRead(out TopicMessage message))
                    {
                        handle.Invoke(message);
                    }
                }
            }
            catch (Exception ex)
            {
                writer.TryComplete(ex);
            }
        });
    }
    public async Task<bool> TryStopAsync()
    {
        var result = _channel.Writer.TryComplete();
        await _readTask;
        return result;
    }

    public static implicit operator ChannelWriter<TopicMessage>(TopicMessageWriterAdapter adapter)
    {
        return adapter.MessageWriter;
    }
}