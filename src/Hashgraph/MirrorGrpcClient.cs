using Grpc.Net.Client;
using Hashgraph.Implementation;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Hashgraph.Test")]

namespace Hashgraph;

public interface IMirrorGrpcClient
{
    /// <summary>
    /// Updates the configuration of this instance of a mirror client thru 
    /// implementation of the supplied <see cref="IMirrorContext"/> callback method.
    /// </summary>
    /// <param name="configure">
    /// The callback method receiving the <see cref="IMirrorContext"/> object providing 
    /// the configuration details of this client instance.  Values can be retrieved 
    /// and set within the context of the method invocation.
    /// </param>
    void Configure(Action<IMirrorContext> configure);

    /// <summary>
    /// Creates a new instance of the mirror client having a shared base configuration with its 
    /// parent.  Changes to the parent’s configuration will reflect in this instances 
    /// configuration while changes in this instances configuration will not be reflected 
    /// in the parent configuration.
    /// </summary>
    /// <param name="configure">
    /// The callback method receiving the <see cref="IMirrorContext"/> object providing 
    /// the configuration details of this client instance.  Values can be retrieved 
    /// and set within the context of the method invocation.
    /// </param>
    /// <returns>
    /// A new instance of a client object.
    /// </returns>
    MirrorGrpcClient Clone(Action<IMirrorContext>? configure = null);

    /// <summary>
    /// .NET Asynchronous dispose method.
    /// </summary>
    /// <remarks>
    /// Closes any GRPC channels solely owned by this <code>Mirror</code> instance.
    /// </remarks>
    /// <returns>
    /// An Async Task.
    /// </returns>
    ValueTask DisposeAsync();
}

/// <summary>
/// Hedera Network Client
/// </summary>
/// <remarks>
/// This component facilitates interaction with the Hedera Mirror Network.  
/// It manages the communication channels with the mirror network and 
/// serialization of requests and responses.  This library generally 
/// shields the client code from directly interacting with the 
/// underlying protobuf communication layer but does provide hooks 
/// allowing advanced low-level manipulation of messages if necessary.
/// </remarks>
public sealed partial class MirrorGrpcClient : IAsyncDisposable, IMirrorGrpcClient
{
    /// <summary>
    /// The context (stack) keeps a memory of configuration and preferences 
    /// within a variety of call contexts.  It can be cloned and tweaked as 
    /// required.  Preferences can be set on cloned or immediate call 
    /// contexts without changing parent contexts.  If a property is not set 
    /// in the current context, the system falls back to the parent context 
    /// for the value, and to its parent until a value has been set.
    /// </summary>
    private readonly MirrorContextStack _context;
    /// <summary>
    /// Creates a new instance of an Hedera Mirror Network Client.
    /// </summary>
    /// <remarks>
    /// Creating a new instance of a <code>Mirror</code> initializes a new instance 
    /// of a client.  It will have a separate cache of GRPC channels to the network 
    /// and will maintain a separate configuration from other clients.  The constructor 
    /// takes an optional callback method that configures the details on how the 
    /// client should connect to the network configuraable details.  See the 
    /// <see cref="IMirrorContext"/> documentation for configuration details.
    /// </remarks>
    /// <param name="configure">
    /// Optional configuration method that can set the location of the network node 
    /// accessing the network and how transaction fees shall be paid for.
    /// </param>
    public MirrorGrpcClient(Action<IMirrorContext>? configure = null) : this(DefaultChannelFactory, configure)
    {
    }
    /// <summary>
    /// Creates a new instance of an Hedera Mirror Network Client with a 
    /// custom gRPC channel factory.
    /// </summary>
    /// <remarks>
    /// Creating a new instance of a <code>Mirror</code> initializes a new instance 
    /// of a client.  It will have a separate cache of GRPC channels to the network 
    /// and will maintain a separate configuration from other clients.  The constructor 
    /// takes an optional callback method that configures the details on how the 
    /// client should connect to the network configuraable details.  See the 
    /// <see cref="IMirrorContext"/> documentation for configuration details.
    /// </remarks>
    /// <param name="channelFactory">
    /// A custom callback method returning a new channel given the target mirror 
    /// node URI.  Note, this method is only called once for each unique URI 
    /// requested by the mirror grpc client (which is a function of the current
    /// context's URI parameter)
    /// </param>
    /// <param name="configure">
    /// Optional configuration method that can set the location of the network node 
    /// accessing the network and how transaction fees shall be paid for.
    /// </param>
    public MirrorGrpcClient(Func<Uri, GrpcChannel> channelFactory, Action<IMirrorContext>? configure = null)
    {
        // Create a Context with System Defaults 
        // that are unreachable and can't be "Reset".
        // At the moment, there are no defaults to set
        // but we still want a "root".
        _context = new MirrorContextStack(new MirrorContextStack(channelFactory));
        configure?.Invoke(_context);
    }
    /// <summary>
    /// Internal implementation of mirror client creation.  Accounts for  newly created 
    /// clients and cloning of clients alike.
    /// </summary>
    /// <param name="channelFactory">
    /// The channel factory method to use when a new gRPC client channel is needed.
    /// </param>
    /// <param name="configure">
    /// The optional <see cref="IContext"/> callback method, passed in from public 
    /// instantiation or a <see cref="MirrorGrpcClient.Clone(Action{IMirrorContext})"/> method call.
    /// </param>
    /// <param name="parent">
    /// The parent <see cref="MirrorContextStack"/> if this creation is a result of a 
    /// <see cref="Client.Clone(Action{IContext})"/> method call.
    /// </param>
    private MirrorGrpcClient(MirrorContextStack parent, Action<IMirrorContext>? configure)
    {
        _context = new MirrorContextStack(parent);
        configure?.Invoke(_context);
    }
    /// <summary>
    /// Updates the configuration of this instance of a mirror client thru 
    /// implementation of the supplied <see cref="IMirrorContext"/> callback method.
    /// </summary>
    /// <param name="configure">
    /// The callback method receiving the <see cref="IMirrorContext"/> object providing 
    /// the configuration details of this client instance.  Values can be retrieved 
    /// and set within the context of the method invocation.
    /// </param>
    public void Configure(Action<IMirrorContext> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure), "Configuration action cannot be null.");
        }
        configure(_context);
    }
    /// <summary>
    /// Creates a new instance of the mirror client having a shared base configuration with its 
    /// parent.  Changes to the parent’s configuration will reflect in this instances 
    /// configuration while changes in this instances configuration will not be reflected 
    /// in the parent configuration.
    /// </summary>
    /// <param name="configure">
    /// The callback method receiving the <see cref="IMirrorContext"/> object providing 
    /// the configuration details of this client instance.  Values can be retrieved 
    /// and set within the context of the method invocation.
    /// </param>
    /// <returns>
    /// A new instance of a client object.
    /// </returns>
    public MirrorGrpcClient Clone(Action<IMirrorContext>? configure = null)
    {
        return new MirrorGrpcClient(_context, configure);
    }
    /// <summary>
    /// Creates a new child context based on the current context instance.  
    /// Includes an optional configuration method that can be immediately 
    /// applied to the new context.  This method is used internally to create 
    /// contexts for cloned clients and network method calls having custom 
    /// configuration callbacks.
    /// </summary>
    private MirrorContextStack CreateChildContext(Action<IMirrorContext>? configure)
    {
        var context = new MirrorContextStack(_context);
        configure?.Invoke(context);
        return context;
    }
    /// <summary>
    /// .NET Asynchronous dispose method.
    /// </summary>
    /// <remarks>
    /// Closes any GRPC channels solely owned by this <code>Mirror</code> instance.
    /// </remarks>
    /// <returns>
    /// An Async Task.
    /// </returns>
    public ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }
    /// <summary>
    /// The default algorithm for creatting channels for the
    /// gRPC mirror streaming client.  This implementation sets
    /// the keep alive timeout of 30s, ping delay 60s and
    /// keep alive policy of always.  Testing has shown this is
    /// the best method for keepign the HCS streaming service alive
    /// for monitoring an HCS stream.
    /// </summary>
    /// <param name="uri">
    /// The URI endpoint of the gRPC mirror node HCS stream.
    /// </param>
    /// <returns>
    /// A GrpcChannel pointing to the URI of the mirror node endpoing.
    /// </returns>
    private static GrpcChannel DefaultChannelFactory(Uri uri)
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
}