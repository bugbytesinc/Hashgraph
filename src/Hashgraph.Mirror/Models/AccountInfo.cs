#pragma warning disable CS8618
using Hashgraph.Mirror.Mappers;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Token information retrieved from a mirror node.
/// </summary>
public class AccountInfo
{
    /// <summary>
    /// The ID of the account
    /// </summary>
    [JsonPropertyName("account")]
    [JsonConverter(typeof(AddressConverter))]
    public Address Account { get; set; }
    /// <summary>
    /// RFC4648 no-padding base32 encoded account alias
    /// </summary>
    [JsonPropertyName("alias")]
    public string Alias { get; set; }
    /// <summary>
    /// Account Auto-Renew Period in seconds.
    /// </summary>
    [JsonPropertyName("auto_renew_period")]
    public long AutoRenewPeriod { get; set; }
    /// <summary>
    /// Structure enumerating the account balance and the
    /// first 100 token balances for the account.
    /// </summary>
    [JsonPropertyName("balance")]
    public AccountBalances Balances { get; set; }
    /// <summary>
    /// Consensus Timestamp when this account was created
    /// </summary>
    [JsonPropertyName("created_timestamp")]
    [JsonConverter(typeof(ConsensusTimeStampConverter))]
    public ConsensusTimeStamp Created { get; set; }
    /// <summary>
    /// Flag indicating that the staking reward is
    /// explicitly declined.
    /// </summary>
    [JsonPropertyName("decline_reward")]
    public bool DeclineReward { get; set; }
    /// <summary>
    /// Flag indicating that the account has been deleted.
    /// </summary>
    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }
    /// <summary>
    /// The account's associated EVM nonce.
    /// </summary>
    [JsonPropertyName("ethereum_nonce")]
    public long Nonce { get; set; }
    /// <summary>
    /// The account's public address encoded
    /// for use with the contract EVM.
    /// </summary>
    [JsonPropertyName("evm_address")]
    public string ContractAddress { get; set; }
    /// <summary>
    /// Timestamp at which the network will try to 
    /// renew the account rent or delete the account
    /// if there are no funds to extends its lifetime.
    /// </summary>
    [JsonPropertyName("expiry_timestamp")]
    [JsonConverter(typeof(ConsensusTimeStampConverter))]
    public ConsensusTimeStamp Expires { get; set; }
    /// <summary>
    /// The public endorsments requied by this account.
    /// </summary>
    [JsonPropertyName("key")]
    [JsonConverter(typeof(EndorsementConverter))]
    public Endorsement Endorsement { get; set; }
    /// <summary>
    /// The number of auto-associations for this account.
    /// </summary>
    [JsonPropertyName("max_automatic_token_associations")]
    public int Associations { get; set; }
    /// <summary>
    /// The account's memo.
    /// </summary>
    [JsonPropertyName("memo")]
    public string Memo { get; set; }
    /// <summary>
    /// The pending reward in tinybars the account will receive in 
    /// the next reward payout. Note the value is updated at the 
    /// end of each staking period and there may be delay to 
    /// reflect the changes in the past staking period.
    /// </summary>
    [JsonPropertyName("pending_reward")]
    public long PendingReward { get; set; }
    /// <summary>
    /// Flag indicating that this account must sign transactions
    /// where this account receives crypto or tokens.
    /// </summary>
    [JsonPropertyName("receiver_sig_required")]
    public bool? ReceiverSignatureRequired { get; set; }
    /// <summary>
    /// The account to which this account is staking
    /// </summary>
    [JsonPropertyName("staked_account_id")]
    [JsonConverter(typeof(AddressConverter))]
    public Address StakedAccount { get; set; }
    /// <summary>
    /// The id of the node to which this account is staking
    /// </summary>
    [JsonPropertyName("staked_node_id")]
    public long? StakedNode { get; set; }
    /// <summary>
    /// The staking period during which either the staking settings 
    /// for this account changed (such as starting staking or 
    /// changing stakedNode) or the most recent reward was earned, 
    /// whichever is later. If this account is not currently staked 
    /// to a node, then the value is null
    /// </summary>
    [JsonPropertyName("stake_period_start")]
    [JsonConverter(typeof(ConsensusTimeStampConverter))]
    public ConsensusTimeStamp SakePeriodStart { get; set; }
}
