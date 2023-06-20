#pragma warning disable CS8618 
using Hashgraph.Converters;
using System;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Represents a transaction retrieved from a mirror node.
/// </summary>
public class TransactionData
{
    [JsonPropertyName("transaction_id")]
    [JsonConverter(typeof(TxIdMirrorConverter))]
    public TxId TxId { get; set; }
    /// <summary>
    /// Sum of network transction fees.
    /// </summary>
    [JsonPropertyName("charged_tx_fee")]
    public long? Fee { get; set; }
    /// <summary>
    /// The transaction’s consensus timestamp.
    /// </summary>
    [JsonPropertyName("consensus_timestamp")]
    public ConsensusTimeStamp Consensus { get; set; }
    /// <summary>
    /// The address of any entity created by this
    /// transaction (such as the created account or contract)
    /// </summary>
    [JsonPropertyName("entity_id")]
    public Address? CreatedEntity { get; set; }
    /// <summary>
    /// The maximum fee the payer account was willing
    /// to spend on this transaction.
    /// </summary>
    [JsonPropertyName("max_fee")]
    [JsonConverter(typeof(FeeLimitFromStringConverter))]
    public long FeeLimit { get; set; }
    /// <summary>
    /// The memo that was attached to this transaction
    /// </summary>
    [JsonPropertyName("memo_base64")]
    [JsonConverter(typeof(Base64StringToBytesConverter))]
    public ReadOnlyMemory<byte> Memo { get; set; }
    /// <summary>
    /// The identifited type of the transaction.
    /// </summary>
    [JsonPropertyName("name")]
    public string? TransactionType { get; set; }
    /// <summary>
    /// The Hedera Gossip Node the submitted this transaction
    /// </summary>
    [JsonPropertyName("node")]
    public Address? GossipNode { get; set; }
    /// <summary>
    /// The nonce of the transaction (if it is a child tx)
    /// </summary>
    [JsonPropertyName("nonce")]
    public long? Nonce { get; set; }
    /// <summary>
    /// The consensus timestamp of this transaction's
    /// parent transaction (if this is a child transaction)
    /// </summary>
    [JsonPropertyName("parent_consensus_timestamp")]
    public ConsensusTimeStamp? ParentConsensus { get; set; }
    /// <summary>
    /// The Status Code of this transaction
    /// </summary>
    [JsonPropertyName("result")]
    public ResponseCode Status { get; set; }
    /// <summary>
    /// Flag indicating that this was a scheduled transaction.
    /// </summary>
    [JsonPropertyName("scheduled")]
    public bool? IsScheduled { get; set; }
    /// <summary>
    /// Enumerates any staking rewards attached
    /// to this transaction.
    /// </summary>
    [JsonPropertyName("staking_reward_transfers")]
    public StakingRewardData[]? StakingRewards { get; set; }
    /// <summary>
    /// Enumerates the tokens that were transfered
    /// as a part of this transaction.
    /// </summary>
    [JsonPropertyName("token_transfers")]
    public TokenTransferData[]? TokenTransfers { get; set; }
    /// <summary>
    /// The transaction’s computed transaction hash.
    /// </summary>
    [JsonPropertyName("transaction_hash")]
    [JsonConverter(typeof(Base64StringToBytesConverter))]
    public ReadOnlyMemory<byte> Hash { get; set; }
    [JsonPropertyName("transfers")]
    public CryptoTransferData[]? CryptoTransfers { get; set; }
    /// <summary>
    /// The valid time to live in seconds
    /// for the transaction.
    /// </summary>
    [JsonPropertyName("valid_duration_seconds")]
    [JsonConverter(typeof(ValidDurationInSecondsConverter))]
    public TimeSpan ValidDuration { get; set; }
    [JsonPropertyName("valid_start_timestamp")]
    public ConsensusTimeStamp ValidStarting { get; set; }
}
