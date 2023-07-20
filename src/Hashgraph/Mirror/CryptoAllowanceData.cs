#pragma warning disable CS8618
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Allowance information retreived from a mirror node.
/// </summary>
public class CryptoAllowanceData
{
    /// <summary>
    /// ID of the token owner.
    /// </summary>
    [JsonPropertyName("owner")]
    public Address Owner { get; set; }
    /// <summary>
    /// ID of the account allowed to spend the token.
    /// </summary>
    [JsonPropertyName("spender")]
    public Address Spender { get; set; }
    /// <summary>
    /// The amount of token the allowed spender may
    /// spend from the owner account (denominated
    /// in smallest denomination)
    /// </summary>
    [JsonPropertyName("amount_granted")]
    public long Amount { get; set; }
}
