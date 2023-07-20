#pragma warning disable CS8618 
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Helper Class for retrieving just the timestamp from the transaction list.
/// </summary>
internal class TransactionTimestampData
{
    /// <summary>
    /// The transaction’s consensus timestamp.
    /// </summary>
    [JsonPropertyName("consensus_timestamp")]
    public ConsensusTimeStamp Consensus { get; set; }
}
