using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Paged transaction list information returned from a mirror node.
/// </summary>
public class TransactionList : PagedList<Transaction>
{
    /// <summary>
    /// List of transactions.
    /// </summary>
    [JsonPropertyName("transactions")]
    public Transaction[]? Transactions { get; set; }
    /// <summary>
    /// Method enumerating the items in the list.
    /// </summary>
    /// <returns>
    /// Enumerable of Transactions.
    /// </returns>
    public override IEnumerable<Transaction> GetItems()
    {
        return Transactions ?? Array.Empty<Transaction>();
    }
}