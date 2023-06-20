using System.Text.Json.Serialization;

namespace Hashgraph.Mirror.Implementation;
/// <summary>
/// Helper class holding the search response for
/// a list of transactions matching the specified id.
/// </summary>
internal class TransactionDetailByIdResponse
{
    /// <summary>
    /// List of transactions found.
    /// </summary>
    [JsonPropertyName("transactions")]
    public TransactionDetailData[]? Transactions { get; set; }
}
