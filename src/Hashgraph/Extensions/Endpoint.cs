using System;

namespace Hashgraph.Extensions;

/// <summary>
/// Represents an Hedera Node Endpoint IP Address and Port
/// </summary>
public sealed record Endpoint
{
    /// <summary>
    /// Presently, the The 32-bit IPv4 address of the 
    /// server node's gRPC access encoded in left to 
    /// right order (e.g. 127.0.0.1 has 127 as its first byte)
    /// </summary>
    public ReadOnlyMemory<byte> IpAddress { get; private set; }
    /// <summary>
    /// The Port number accessing the server node's gRPC service.
    /// </summary>
    public int Port { get; private set; }
    /// <summary>
    /// Internal constructor from Raw Protobuf Results
    /// </summary>
    internal Endpoint(Proto.ServiceEndpoint serviceEndpoint)
    {
        if (serviceEndpoint is not null)
        {
            IpAddress = serviceEndpoint.IpAddressV4.Memory;
            Port = serviceEndpoint.Port;
        }
        // Defaults to Empty otherwise.
    }
}