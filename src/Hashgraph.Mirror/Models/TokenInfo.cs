#pragma warning disable CS8618
using Hashgraph.Mirror.Mappers;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Token information retrieved from a mirror node.
/// </summary>
public class TokenInfo
{
    /// <summary>
    /// The token ID.
    /// </summary>
    [JsonPropertyName("token_id")]
    [JsonConverter(typeof(AddressConverter))]
    public Address TokenId { get; set; }
    /// <summary>
    /// The token’s symbol.
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }
    /// <summary>
    /// The token’s name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// The number of token decimals (if this is a fungible token).
    /// </summary>
    [JsonPropertyName("decimals")]
    public int Decimals { get; set; }
    /// <summary>
    /// The total supply of the token (in tinytokens).
    /// </summary>
    [JsonPropertyName("total_supply")]
    public long Circulation { get; set; }
    /// <summary>
    /// The last time this token was modified.
    /// </summary>
    [JsonPropertyName("modified_timestamp")]
    [JsonConverter(typeof(ConsensusTimeStampConverter))]
    public ConsensusTimeStamp Modified { get; set; }
}
