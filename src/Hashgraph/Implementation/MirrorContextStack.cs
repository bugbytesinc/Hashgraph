using Google.Protobuf;
using Grpc.Net.Client;
using System;
using System.Linq;
using System.Net.Http;

namespace Hashgraph.Implementation;

/// <summary>
/// Internal Implementation of the <see cref="IContext"/> used for configuring
/// <see cref="Client"/> objects.  Maintains a stack of parent objects 
/// and coordinates values returned for various contexts.  Not intended for
/// public use.
/// </summary>
internal class MirrorContextStack : ContextStack<GossipContextStack>, IMirrorContext
{
    public Uri Uri { get => get<Uri>(nameof(Uri)); set => set(nameof(Uri), value); }
    public Action<IMessage>? OnSendingRequest { get => get<Action<IMessage>>(nameof(OnSendingRequest)); set => set(nameof(OnSendingRequest), value); }

    public MirrorContextStack(MirrorContextStack? parent) : base(parent) { }
    protected override bool IsValidPropertyName(string name)
    {
        switch (name)
        {
            case nameof(Uri):
            case nameof(OnSendingRequest):
                return true;
            default:
                return false;
        }
    }
    protected override Uri GetChannelUrl()
    {
        var uri = Uri;
        if (uri is null)
        {
            throw new InvalidOperationException("The Mirror Node Url has not been configured.");
        }
        return uri;
    }
    protected override GrpcChannel ConstructNewChannel(Uri uri)
    {
        var options = new GrpcChannelOptions()
        {
            HttpHandler = new SocketsHttpHandler
            {
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always
            }
        };
        return GrpcChannel.ForAddress(uri, options);
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