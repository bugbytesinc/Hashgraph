using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror.Implementation;
/// <summary>
/// Paged list of crypto (hbar) allowances
/// </summary>
internal class CryptoAllowanceDataPage : Page<CryptoAllowanceData>
{
    /// <summary>
    /// List of crypto (hbar) allowances
    /// </summary>
    [JsonPropertyName("allowances")]
    public CryptoAllowanceData[]? CryptoAllowances { get; set; }
    /// <summary>
    /// Enumerates the list of token allowances.
    /// </summary>
    /// <returns>
    /// An enumerator listing the token allowance records in the list.
    /// </returns>
    public override IEnumerable<CryptoAllowanceData> GetItems()
    {
        return CryptoAllowances ?? Array.Empty<CryptoAllowanceData>();
    }
}
