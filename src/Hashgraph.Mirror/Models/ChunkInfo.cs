#pragma warning disable CS8618
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;

/// <summary>
/// HCS Chunk Information that may or may not be part of this message.
/// </summary>
public class ChunkInfo
{
    /// <summary>
    /// Corresponding initial transaction id.
    /// </summary>
    [JsonPropertyName("initial_transaction_id")]
    public string InitialTransactionId { get; set; }
    /// <summary>
    /// Chunk number.
    /// </summary>
    [JsonPropertyName("number")]
    public int Number { get; set; }
    /// <summary>
    /// Total number of chunks.
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

}