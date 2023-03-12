#pragma warning disable CS8618 
using Hashgraph.Mirror.Mappers;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Represents a transaction retrieved from a mirror node.
/// </summary>
public class Transaction
{
    /// <summary>
    /// The transaction’s consensus timestamp.
    /// </summary>
    [JsonPropertyName("consensus_timestamp")]
    [JsonConverter(typeof(ConsensusTimeStampConverter))]
    public ConsensusTimeStamp TimeStamp { get; set; }
    /// <summary>
    /// The transaction’s computed transaction hash.
    /// </summary>
    [JsonPropertyName("transaction_hash")]
    public string Hash { get; set; }
}
