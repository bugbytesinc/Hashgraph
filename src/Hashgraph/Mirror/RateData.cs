using Hashgraph.Converters;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Represents Hedera hBar rate exchange with USD
/// </summary>
public class RateData
{
    /// <summary>
    /// The value of USD in cents of this 
    /// exchange rate fraction.
    /// </summary>
    [JsonPropertyName("cent_equivalent")]
    public int CentEquivalent { get; set; }
    /// <summary>
    /// The value of hBars in tinybars of
    /// this exchange rate fraction.
    /// </summary>
    [JsonPropertyName("hbar_equivalent")]
    public int HbarEquivalent { get; set; }
    /// <summary>
    /// Time at which this exchange rate is
    /// no longer valid.
    /// </summary>
    [JsonPropertyName("expiration_time")]
    [JsonConverter(typeof(ConsensusTimeStampFromLongConverter))]
    public ConsensusTimeStamp Expiration { get; set; }

}