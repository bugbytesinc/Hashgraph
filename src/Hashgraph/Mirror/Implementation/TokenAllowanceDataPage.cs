using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror.Implementation;
/// <summary>
/// Paged list of token allowances
/// </summary>
internal class TokenAllowanceDataPage : Page<TokenAllowanceData>
{
    /// <summary>
    /// List of token allowances
    /// </summary>
    [JsonPropertyName("allowances")]
    public TokenAllowanceData[]? TokenAllowances { get; set; }
    /// <summary>
    /// Enumerates the list of token allowances.
    /// </summary>
    /// <returns>
    /// An enumerator listing the token allowance records in the list.
    /// </returns>
    public override IEnumerable<TokenAllowanceData> GetItems()
    {
        return TokenAllowances ?? Array.Empty<TokenAllowanceData>();
    }
}
