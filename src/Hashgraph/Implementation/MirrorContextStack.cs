using Google.Protobuf;
using Grpc.Core;
using System;
using System.Linq;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal Implementation of the <see cref="IContext"/> used for configuring
    /// <see cref="Client"/> objects.  Maintains a stack of parent objects 
    /// and coordinates values returned for various contexts.  Not intended for
    /// public use.
    /// </summary>
    internal class MirrorContextStack : ContextStack<GossipContextStack>, IMirrorContext
    {
        public string Url { get => get<string>(nameof(Url)); set => set(nameof(Url), value); }
        public Action<IMessage>? OnSendingRequest { get => get<Action<IMessage>>(nameof(OnSendingRequest)); set => set(nameof(OnSendingRequest), value); }

        public MirrorContextStack(MirrorContextStack? parent) : base(parent) { }
        protected override bool IsValidPropertyName(string name)
        {
            switch (name)
            {
                case nameof(Url):
                case nameof(OnSendingRequest):
                    return true;
                default:
                    return false;
            }
        }
        protected override string GetChannelUrl()
        {
            var url = Url;
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidOperationException("The Mirror Node Url has not been configured.");
            }
            return url;
        }
        protected override Channel ConstructNewChannel(string url)
        {
            var options = new[] { new ChannelOption("grpc.keepalive_permit_without_calls", 1) };
            return new Channel(url, ChannelCredentials.Insecure, options);
        }

        public Action<IMessage> InstantiateOnSendingRequestHandler()
        {
            var handlers = GetAll<Action<IMessage>>(nameof(OnSendingRequest)).Where(h => h != null).ToArray();
            if (handlers.Length > 0)
            {
                return (IMessage request) => ExecuteHandlers(handlers, request);
            }
            else
            {
                return NoOp;
            }
            static void ExecuteHandlers(Action<IMessage>[] handlers, IMessage request)
            {
                var data = new ReadOnlyMemory<byte>(request.ToByteArray());
                foreach (var handler in handlers)
                {
                    handler(request);
                }
            }
            static void NoOp(IMessage request)
            {
            }
        }
    }
}
