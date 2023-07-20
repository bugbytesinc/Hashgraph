using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror.Implementation;
/// <summary>
/// Paged transaction list information returned from a mirror node.
/// </summary>
internal class TransactionTimestampDataPage : Page<TransactionTimestampData>
{
    /// <summary>
    /// List of transactions.
    /// </summary>
    [JsonPropertyName("transactions")]
    public TransactionTimestampData[]? Transactions { get; set; }
    /// <summary>
    /// Method enumerating the items in the list.
    /// </summary>
    /// <returns>
    /// Enumerable of Transactions.
    /// </returns>
    public override IEnumerable<TransactionTimestampData> GetItems()
    {
        return Transactions ?? Array.Empty<TransactionTimestampData>();
    }
}