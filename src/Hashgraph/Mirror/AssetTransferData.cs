#pragma warning disable CS8618 
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;

/// <summary>
/// Represents an Asset (NFT) transfer within a transaction.
/// </summary>
public class AssetTransferData
{
    /// <summary>
    /// The identifier of the asset token type transferred.
    /// </summary>
    [JsonPropertyName("token_id")]
    public Address Token { get; set; }
    /// <summary>
    /// The serial number of the Asset (NFT) transferred.
    /// </summary>
    [JsonPropertyName("serial_number")]
    public long SerialNumber { get; set; }
    /// <summary>
    /// The account sending the asset.
    /// </summary>
    [JsonPropertyName("sender_account_id")]
    public Address Sender { get; set; }
    /// <summary>
    /// The account receiving the asset.
    /// </summary>
    [JsonPropertyName("receiver_account_id")]
    public Address Receiver { get; set; }
    /// <summary>
    /// Flag indiciating this transfer was performed
    /// as an allowed transfer by a third party account.
    /// </summary>
    [JsonPropertyName("is_approval")]
    public bool IsAllowance { get; set; }
}
