using Hashgraph.Converters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror.Implementation;
/// <summary>
/// Paged list of balance objects returned from the mirror node.
/// </summary>
internal class AccountBalancePage : Page<AccountBalanceData>
{
    /// <summary>
    /// The timestamp at which this information was valid.
    /// </summary>
    [JsonPropertyName("timestamp")]
    [JsonConverter(typeof(ConsensusTimeStampConverter))]
    public ConsensusTimeStamp TimeStamp { get; set; }
    /// <summary>
    /// List of balances for returned by the mirror node query.
    /// </summary>
    [JsonPropertyName("balances")]
    public AccountBalanceData[]? Balances { get; set; }
    /// <summary>
    /// Enumerates the list of balances.
    /// </summary>
    /// <returns>
    /// Enumerator for the TokenBalance objects in the list.
    /// </returns>
    public override IEnumerable<AccountBalanceData> GetItems()
    {
        return Balances ?? Array.Empty<AccountBalanceData>();
    }
}