#pragma warning disable CS8618 
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;

/// <summary>
/// Represents a token transfer within a transaction.
/// </summary>
public class TokenTransferData
{
    /// <summary>
    /// The identifier of the token transferred.
    /// </summary>
    [JsonPropertyName("token_id")]
    public Address Token { get; set; }
    /// <summary>
    /// The account sending or receiving the token.
    /// </summary>
    [JsonPropertyName("account")]
    public Address Account { get; set; }
    /// <summary>
    /// The amount of token transferred (positive
    /// value means the account received token, 
    /// negative value means the account sent the token)
    /// </summary>
    [JsonPropertyName("amount")]
    public long Amount { get; set; }
    /// <summary>
    /// Flag indiciating this transfer was performed
    /// as an allowed transfer by a third party account.
    /// </summary>
    [JsonPropertyName("is_approval")]
    public bool IsAllowance { get; set; }
}
