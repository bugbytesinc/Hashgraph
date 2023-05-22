using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror.Implementation;
/// <summary>
/// Contains a paged list of Token Holdings.
/// </summary>
internal class TokenHoldingDataPage : Page<TokenHoldingData>
{
    /// <summary>
    /// List of token holding data records.
    /// </summary>
    [JsonPropertyName("tokens")]
    public TokenHoldingData[]? TokenHoldings { get; set; }
    /// <summary>
    /// Enumerates the list of token holdings.
    /// </summary>
    /// <returns>
    /// An enumerator listing the token holding records in the list.
    /// </returns>
    public override IEnumerable<TokenHoldingData> GetItems()
    {
        return TokenHoldings ?? Array.Empty<TokenHoldingData>();
    }
}