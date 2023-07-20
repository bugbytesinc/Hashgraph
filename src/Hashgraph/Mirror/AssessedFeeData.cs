#pragma warning disable CS8618 
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;

/// <summary>
/// Represents custom assessed fees imposed as a result
/// of a token transfer.
/// </summary>
public class AssessedFeeData
{
    /// <summary>
    /// The amount of token or crypto assessed
    /// </summary>
    [JsonPropertyName("amount")]
    public long Amount { get; set; }
    /// <summary>
    /// The account receiving the token or crypto fee.
    /// </summary>
    [JsonPropertyName("collector_account_id")]
    public Address Collector { get; set; }
    /// <summary>
    /// The accounts paying the token or crypto fee.
    /// </summary>
    [JsonPropertyName("effective_payer_account_ids")]
    public Address[] Payers { get; set; }
    /// <summary>
    /// Address of the token transferred, or None
    /// for tinybars.
    /// </summary>
    [JsonPropertyName("token_id")]
    public Address Token { get; set; }
}