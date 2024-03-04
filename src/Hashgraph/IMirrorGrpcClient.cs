using System;
using System.Threading.Tasks;
using Grpc.Net.Client;

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
    ///  Gets a GRPC channel to the mirror node.  The channel is cached and reused
    /// </summary>
    /// <param name="configure"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    GrpcChannel GetChannel(Action<IMirrorContext>? configure);

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

    // TODO: Remove this
    Task SubscribeTopicAsync(SubscribeTopicParams subscribeParameters, Action<IMirrorContext>? configure = null);
}