#pragma warning disable CS8618 
using Hashgraph.Mirror.Mappers;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Represents a token balance entry for an account and token.
/// </summary>
public class TokenAccountBalance
{
    /// <summary>
    /// The account holding the token.
    /// </summary>
    [JsonPropertyName("account")]
    [JsonConverter(typeof(AddressConverter))]
    public Address Account { get; set; }
    /// <summary>
    /// The balance of account’s holdings of token in tinytokens.
    /// </summary>

    [JsonPropertyName("balance")]
    public long Balance { get; set; }
}
