using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror.Implementation;
/// <summary>
/// Paged transaction list information returned from a mirror node.
/// </summary>
internal class TransactionDataPage : Page<TransactionData>
{
    /// <summary>
    /// List of transactions.
    /// </summary>
    [JsonPropertyName("transactions")]
    public TransactionData[]? Transactions { get; set; }
    /// <summary>
    /// Method enumerating the items in the list.
    /// </summary>
    /// <returns>
    /// Enumerable of Transactions.
    /// </returns>
    public override IEnumerable<TransactionData> GetItems()
    {
        return Transactions ?? Array.Empty<TransactionData>();
    }
}