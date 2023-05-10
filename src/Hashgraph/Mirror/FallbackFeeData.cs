#pragma warning disable CS8618
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;

/// <summary>
/// Fee charged when no value was exchanged
/// for an asset (nft) with a royalty fee.
/// </summary>
public class FallbackFeeData
{
    /// <summary>
    /// Amount of fixed fee to collect.
    /// </summary>
    [JsonPropertyName("amount")]
    public long Amount { get; set; }
    /// <summary>
    /// The token that the fee is denominated
    /// in, or NONE for hBar.
    /// </summary>
    [JsonPropertyName("denominating_token_id")]
    public Address FeeToken { get; set; } = default!;
}