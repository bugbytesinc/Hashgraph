#pragma warning disable CS8618 
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Represents a transaction detail from a mirror node.
/// Similar to the TransactionData object but includes
/// custom fees and Asset (NFT) transfer data.
/// </summary>
public class TransactionDetailData : TransactionData
{
    /// <summary>
    /// Assessed custom fees for transferring tokens.
    /// </summary>
    [JsonPropertyName("assessed_custom_fees")]
    public AssessedFeeData[]? AssessedFees { get; set; }
    /// <summary>
    /// List of Assets transferred as a part of this
    /// transaction.
    /// </summary>
    [JsonPropertyName("nft_transfers")]
    public AssetTransferData[]? AssetTransfers { get; set; }
}
