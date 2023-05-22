using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;

/// <summary>
/// Represents a consensus timestamp range.
/// </summary>
public class TimestampRangeData
{
    /// <summary>
    /// The starting timestamp that this
    /// time range is valid for (inclusive).
    /// </summary>
    [JsonPropertyName("from")]
    public ConsensusTimeStamp? Starting { get; set; }
    /// <summary>
    /// The ending timestamp that this time
    /// range is valid for (exclusive).
    /// </summary>
    [JsonPropertyName("to")]
    public ConsensusTimeStamp? Ending { get; set; }
}