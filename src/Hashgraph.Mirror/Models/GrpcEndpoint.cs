using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Represents a gossip node’s gRPC endpoint.
/// </summary>
public class GrpcEndpoint
{
    /// <summary>
    /// IPV4 address of the endpoint.
    /// </summary>
    [JsonPropertyName("ip_address_v4")]
    public string? Address { get; set; }
    /// <summary>
    /// Port number to connect with.
    /// </summary>
    [JsonPropertyName("port")]
    public int Port { get; set; }
}