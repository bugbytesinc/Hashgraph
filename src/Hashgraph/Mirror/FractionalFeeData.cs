#pragma warning disable CS8618
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;

/// <summary>
/// Represents a fracional royalty fee.
/// </summary>
public class FractionalFeeData
{
    /// <summary>
    /// Are collecor accounts exempt from paying fees.
    /// </summary>
    [JsonPropertyName("all_collectors_are_exempt")]
    public bool CollectorsExempt { get; set; }
    /// <summary>
    /// Amount of fractional fee to collect.
    /// </summary>
    [JsonPropertyName("amount")]
    public FractionData Amount { get; set; }
    /// <summary>
    /// The account receiving the fees.
    /// </summary>
    [JsonPropertyName("collector_account_id")]
    public Address Collector { get; set; } = default!;
    /// <summary>
    /// The token that the fee is denominated
    /// in, or NONE for hBar.
    /// </summary>
    [JsonPropertyName("denominating_token_id")]
    public Address FeeToken { get; set; } = default!;
    /// <summary>
    /// Maximum Charged Fee
    /// </summary>
    [JsonPropertyName("maximum")]
    public long Maximum { get; set; }
    /// <summary>
    /// Minimum Charged Fee
    /// </summary>
    [JsonPropertyName("minimum")]
    public long Minimum { get; set; }
    /// <summary>
    /// Flag indicating the sender pays fees
    /// instead of the receiver.
    /// </summary>
    [JsonPropertyName("net_of_transfers")]
    public bool NetOfTransfers { get; set; }
}
