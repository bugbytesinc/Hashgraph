#pragma warning disable CS8618
using Hashgraph.Mirror.Mappers;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;

/// <summary>
/// Structure identifying the hBar balance for
/// an associated account and the balances of the
/// first 100 tokens held by the account.
/// </summary>
/// <remarks>
/// NOTE this structure may not provide the entire
/// listing of tokens held by the associated acocunt.
/// </remarks>
public class AccountBalances
{
    /// <summary>
    /// Timestamp corresponding to this balance snapshot.
    /// </summary>
    [JsonPropertyName("timestamp")]
    [JsonConverter(typeof(ConsensusTimeStampConverter))]
    public ConsensusTimeStamp TimeStamp { get; set; }
    /// <summary>
    /// Crypto balance in tinybars.
    /// </summary>
    [JsonPropertyName("balance")]
    public long Balance { get; set; }
    /// <summary>
    /// Listing of the first 100 token balance values
    /// for the associated account.
    /// </summary>
    [JsonPropertyName("tokens")]
    public AccountTokenBalance[] Tokens { get; set; }
}
