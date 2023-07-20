#pragma warning disable CS8618 
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;

/// <summary>
/// Represents an hBar transfer within a transaction.
/// </summary>
public class CryptoTransferData
{
    /// <summary>
    /// The account sending or receiving hBar.
    /// </summary>
    [JsonPropertyName("account")]
    public Address Account { get; set; }
    /// <summary>
    /// The amount tinybars transferred (a positive
    /// value means the account received tinybars, 
    /// negative value means the account sent tinybars)
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