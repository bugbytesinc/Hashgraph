using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Hashgraph.Test.Fixtures
{
    public sealed class TopicMessageCapture
    {
        private Channel<TopicMessage> _channel;
        private readonly Task _readTask;
        private int _limit;
        private List<TopicMessage> _list;

        public ChannelWriter<TopicMessage> MessageWriter { get { return _channel.Writer; } }
        public IList<TopicMessage> CapturedList { get { return _list; } }
        public TopicMessageCapture(int limit)
        {
            _limit = limit;
            _list = new List<TopicMessage>(limit);
            _channel = Channel.CreateBounded<TopicMessage>(Math.Min(32, limit));
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
                            _list.Add(message);
                            if (_list.Count == _limit - 1)
                            {
                                writer.TryComplete();
                                return;
                            }
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

        public static implicit operator ChannelWriter<TopicMessage>(TopicMessageCapture adapter)
        {
            return adapter.MessageWriter;
        }
    }
}
