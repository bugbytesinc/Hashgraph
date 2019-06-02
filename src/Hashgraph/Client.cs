using Grpc.Core;
using Hashgraph.Implementation;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Hashgraph.Test")]
namespace Hashgraph
{
    /// <summary>
    /// Hedera Network Client
    /// </summary>
    /// <remarks>
    /// This component facilitates interaction with the Hedera Network.  
    /// It manages the communication channels with the network and 
    /// serialization of requests and responses.  This library generally 
    /// shields the client code from directly interacting with the 
    /// underlying protobuf communication layer but does provide hooks 
    /// allowing advanced low-level manipulation of messages if necessary.
    /// </remarks>
    public sealed partial class Client : IAsyncDisposable
    {
        /// <summary>
        /// The context (stack) keeps a memory of configuration and preferences 
        /// within a variety of call contexts.  It can be cloned and tweaked as 
        /// required.  Preferences can be set on cloned or immediate call 
        /// contexts without changing parent contexts.  If a property is not set 
        /// in the current context, the system falls back to the parent context 
        /// for the value, and to its parent until a value has been set.
        /// </summary>
        private readonly ContextStack _context;
        /// <summary>
        /// Retains a map of open channels.  GRPC documentation suggests the most 
        /// expensive action to take when interacting with a GRPC service is the 
        /// opening of the channel.  Channels are cached only by URI at the moment, 
        /// since the library does not provide any customization of communication 
        /// channels at this time. Cloning a channel results in a new empty map of 
        /// channels.  Cloned <code>Client</code>s share a root configuration, but 
        /// have a separate channel cache.
        /// </summary>
        private readonly ConcurrentDictionary<string, Channel> _channels;
        /// <summary>
        /// Creates a new instance of an Hedera Network Client.
        /// </summary>
        /// <remarks>
        /// Creating a new instance of a <code>Client</code> initializes a new instance 
        /// of a client.  It will have a separate cache of GRPC channels to the network 
        /// and will maintain a separate configuration from other clients.  The constructor 
        /// takes an optional callback method that configures the details on how the 
        /// client should connect to the network and what accounts generally pay 
        /// transaction fees and other details.  See the <see cref="IContext"/> documentation 
        /// for configuration details.
        /// </remarks>
        /// <param name="configure">
        /// Optional configuration method that can set the location of the network node 
        /// accessing the network and how transaction fees shall be paid for.
        /// </param>
        public Client(Action<IContext>? configure = null) : this(configure, null)
        {
        }
        /// <summary>
        /// Internal implementation of client creation.  Accounts for  newly created 
        /// clients and cloning of clients alike.
        /// </summary>
        /// <param name="configure">
        /// The optional <see cref="IContext"/> callback method, passed in from public 
        /// instantiation or a <see cref="Client.Clone(Action{IContext})"/> method call.
        /// </param>
        /// <param name="parent">
        /// The parent <see cref="ContextStack"/> if this creation is a result of a 
        /// <see cref="Client.Clone(Action{IContext})"/> method call.
        /// </param>
        private Client(Action<IContext>? configure, ContextStack? parent)
        {
            if (parent is null)
            {
                // Create a Context with System Defaults 
                // that are unreachable and can't be "Reset".
                parent = new ContextStack(null)
                {
                    FeeLimit = 100000,
                    TransactionDuration = TimeSpan.FromSeconds(120),
                    RetryCount = 5,
                    RetryDelay = TimeSpan.FromMilliseconds(200),
                    AdjustForLocalClockDrift = false
                };
            }
            _context = new ContextStack(parent);
            configure?.Invoke(_context);
            _channels = new ConcurrentDictionary<string, Channel>();
        }
        /// <summary>
        /// Updates the configuration of this instance of a client thru 
        /// implementation of the supplied <see cref="IContext"/> callback method.
        /// </summary>
        /// <param name="configure">
        /// The callback method receiving the <see cref="IContext"/> object providing 
        /// the configuration details of this client instance.  Values can be retrieved 
        /// and set within the context of the method invocation.
        /// </param>
        public void Configure(Action<IContext> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure), "Configuration action cannot be null.");
            }
            configure(_context);
        }
        /// <summary>
        /// Creates a new instance of the client having a shared base configuration with its 
        /// parent.  Changes to the parent’s configuration will reflect in this instances 
        /// configuration while changes in this instances configuration will not be reflected 
        /// in the parent configuration.
        /// </summary>
        /// <param name="configure">
        /// The callback method receiving the <see cref="IContext"/> object providing 
        /// the configuration details of this client instance.  Values can be retrieved 
        /// and set within the context of the method invocation.
        /// </param>
        /// <returns>
        /// A new instance of a client object.
        /// </returns>
        public Client Clone(Action<IContext>? configure = null)
        {
            return new Client(configure, _context);
        }
        /// <summary>
        /// Creates a new child context based on the current context instance.  
        /// Includes an optional configuration method that can be immediately 
        /// applied to the new context.  This method is used internally to create 
        /// contexts for cloned clients and network method calls having custom 
        /// configuration callbacks.
        /// </summary>
        private ContextStack CreateChildContext(Action<IContext>? configure)
        {
            var context = new ContextStack(_context);
            configure?.Invoke(context);
            return context;
        }
        /// <summary>
        /// .NET Asynchronous dispose method.
        /// </summary>
        /// <remarks>
        /// Closes any GRPC channels owned by this <code>Client</code> instance.
        /// </remarks>
        /// <returns>
        /// An Async Task.
        /// </returns>
        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_channels.Values.Select(channel => channel.ShutdownAsync()).ToArray());
            _channels.Clear();
        }
    }
}
