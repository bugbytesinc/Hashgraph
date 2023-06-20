#pragma warning disable CS8618 
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror
{
    /// <summary>
    /// Represents a staking award attachted
    /// to a transaction.
    /// </summary>
    public class StakingRewardData
    {
        /// <summary>
        /// The account receiving the staking reward
        /// </summary>
        [JsonPropertyName("account")]
        public Address Account { get; set; }
        /// <summary>
        /// The amount of the staking reward in tinybars
        /// </summary>
        [JsonPropertyName("amount")]
        public long Amount { get; set; }
    }
}
