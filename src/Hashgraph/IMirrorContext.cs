using Google.Protobuf;
using System;

namespace Hashgraph;

/// <summary>
/// A <see cref="MirrorClient"/> instance’s configuration.
/// </summary>
/// <remarks>
/// This interface exposes the current configuration context for a 
/// <see cref="MirrorClient"/> instance.  When accessed thru a 
/// <see cref="MirrorClient.Configure(Action{IMirrorContext})"/>, 
/// <see cref="MirrorClient.Clone(Action{IMirrorContext}?)"/> or one of the 
/// network request methods, calling code can interrogate the 
/// object for configuration details and update as necessary.  
/// Typically, the bare minimum that must be configured in a 
/// context in order to access the mirror network is the URL
/// of a mirror node server <see cref="IMirrorContext.Url"/>.
/// The other default values are typically suitable for most 
/// interactions with the mirror network.
/// </remarks>
public interface IMirrorContext
{
    /// <summary>
    /// gRPC Address identifying a mirror node for
    /// access to the Hedera Mirror Network.
    /// </summary>
    string Url { get; set; }
    /// <summary>
    /// Called by the library just before the serialized protobuf 
    /// is sent to the Mirror Node.  This is the only exposure 
    /// the library provides to the underlying protobuf implementation 
    /// (although they are publicly available in the library).  
    /// This method can be useful for logging and tracking purposes. 
    /// It could also be used in advanced scenarios to modify the 
    /// protobuf message just before sending.
    /// </summary>
    Action<IMessage>? OnSendingRequest { get; set; }
    /// <summary>
    /// Clears a property on the context.  This is the only way to clear 
    /// a property value on the context so that a parent value can be 
    /// used instead.  Setting a value to <code>null</code> does not 
    /// clear the value, it sets it to <code>null</code> for the 
    /// current context and child contexts. 
    /// </summary>
    /// <param name="name">
    /// The name of the property to reset, must be one of the public 
    /// properties of the <code>IContext</code>.  We suggest using 
    /// the <code>nameof()</code> operator to ensure type safety.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// when an invalid <code>name</code> is provided.
    /// </exception>
    void Reset(string name);
}