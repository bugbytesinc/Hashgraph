#pragma warning disable CS8618
using Hashgraph.Mirror.Mappers;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;

/// <summary>
/// Represents a token balance entry for a given account
/// </summary>
public class AccountTokenBalance
{
    /// <summary>
    /// The account holding the token.
    /// </summary>
    [JsonPropertyName("token_id")]
    [JsonConverter(typeof(AddressConverter))]
    public Address Token { get; set; }
    /// <summary>
    /// The balance of account’s holdings of token in tinytokens.
    /// </summary>

    [JsonPropertyName("balance")]
    public long Balance { get; set; }
}
