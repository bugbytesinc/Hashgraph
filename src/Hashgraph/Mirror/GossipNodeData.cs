using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Represents gossip node information returned from the mirror node.
/// </summary>
public class GossipNodeData
{
    /// <summary>
    /// The gossip nodes account ID (for payment purposes).
    /// </summary>
    [JsonPropertyName("node_account_id")]
    public Address Account { get; set; } = default!;
    /// <summary>
    /// A list of gRPC endpoints this gossip node can reached through.
    /// </summary>
    [JsonPropertyName("service_endpoints")]
    public GrpcEndpointData[] Endpoints { get; set; } = default!;
    /// <summary>
    /// Memo associated with the address book
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = default!;
    /// <summary>
    /// File ID associated with this node.
    /// </summary>
    [JsonPropertyName("file_id")]
    public Address File { get; set; } = default!;
    /// <summary>
    /// The minimum stake (rewarded or not rewarded) this 
    /// node must reach before having non-zero consensus weight.
    /// </summary>
    [JsonPropertyName("min_stake")]
    public long MinimumStake { get; set; }
    /// <summary>
    /// The maximum stake (rewarded or not rewarded) this node 
    /// can have as consensus weight
    /// </summary>
    [JsonPropertyName("max_stake")]
    public long MaximumStake { get; set; }
    /// <summary>
    /// Memo associated with this node.
    /// </summary>
    [JsonPropertyName("memo")]
    public string Memo { get; set; } = default!;
    /// <summary>
    /// The Node's ID Number
    /// </summary>
    [JsonPropertyName("node_id")]
    public long NodeId { get; set; }
    /// <summary>
    /// hex encoded hash of the node's TLS certificate
    /// </summary>
    [JsonPropertyName("node_cert_hash")]
    public string CertificateHash { get; set; } = default!;
    /// <summary>
    /// hex encoded X509 RSA public key used to 
    /// verify stream file signature
    /// </summary>
    [JsonPropertyName("public_key")]
    public string PublicKey { get; set; } = default!;
    /// <summary>
    /// The total tinybars earned by this node per whole 
    /// hbar in the last staking period
    /// </summary>
    [JsonPropertyName("reward_rate_start")]
    public long RewardRateStart { get; set; }
    /// <summary>
    /// The node consensus weight at the 
    /// beginning of the staking period
    /// </summary>
    [JsonPropertyName("stake")]
    public long Stake { get; set; }
    /// <summary>
    /// The sum (balance + stakedToMe) for all accounts 
    /// staked to this node with declineReward=true at 
    /// the beginning of the staking period
    /// </summary>
    [JsonPropertyName("stake_not_rewarded")]
    public long StakeNotRewarded { get; set; }
    /// <summary>
    /// The sum (balance + staked) for all accounts staked 
    /// to the node that are not declining rewards at the 
    /// beginning of the staking period
    /// </summary>
    [JsonPropertyName("stake_rewarded")]
    public long StakeRewarded { get; set; }
    /// <summary>
    /// The range of time this data record
    /// is valid for.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public TimestampRangeData ValidRange { get; set; } = default!;
}
