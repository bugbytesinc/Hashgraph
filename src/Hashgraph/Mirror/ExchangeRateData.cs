#pragma warning disable CS8618
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Represents the current and next exchange
/// rate as reporte by the hedera network.
/// </summary>
public class ExchangeRateData
{
    /// <summary>
    /// The current exchange rate that 
    /// the hedera network uses to
    /// convert fees into hBar equivalent.
    /// </summary>
    [JsonPropertyName("current_rate")]
    public RateData CurrentRate { get; set; }
    /// <summary>
    /// The next exchange rate that will
    /// be used by the hedera network to
    /// convert fees into hBar equivalent.
    /// </summary>
    [JsonPropertyName("next_rate")]
    public RateData NextRate { get; set; }
    /// <summary>
    /// Timestamp at which this information
    /// was generated.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public ConsensusTimeStamp Timestamp { get; set; }
}
