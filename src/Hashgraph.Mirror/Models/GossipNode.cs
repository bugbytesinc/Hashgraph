#pragma warning disable CS8618 
using Hashgraph.Mirror.Mappers;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Represents gossip node information returned from the mirror node.
/// </summary>
public class GossipNode
{
    /// <summary>
    /// The gossip nodes account ID (for payment purposes).
    /// </summary>
    [JsonPropertyName("node_account_id")]
    [JsonConverter(typeof(AddressConverter))]
    public Address Account { get; set; }
    /// <summary>
    /// A list of gRPC endpoints this gossip node can reached through.
    /// </summary>

    [JsonPropertyName("service_endpoints")]
    public GrpcEndpoint[] Endpoints { get; set; }
}
