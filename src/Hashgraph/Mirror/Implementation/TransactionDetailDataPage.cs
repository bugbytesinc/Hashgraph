using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror.Implementation;
/// <summary>
/// Paged list of account transaction details.
/// </summary>
internal class TransactionDetailDataPage : Page<TransactionDetailData>
{
    /// <summary>
    /// List of account transaction details.
    /// </summary>
    [JsonPropertyName("transactions")]
    public TransactionDetailData[]? Transactions { get; set; }
    /// <summary>
    /// Enumerates the list of account transaction detail objects.
    /// </summary>
    /// <returns>
    /// Enumerator of account transaction detail objects for this
    /// paged list.
    /// </returns>
    public override IEnumerable<TransactionDetailData> GetItems()
    {
        return Transactions ?? Array.Empty<TransactionDetailData>();
    }
}
