using Google.Protobuf.Collections;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hashgraph;

/// <summary>
/// The details returned from the network after consensus 
/// has been reached for a network request.
/// </summary>
public record TransactionRecord : TransactionReceipt
{
    /// <summary>
    /// Hash of the Transaction.
    /// </summary>
    public ReadOnlyMemory<byte> Hash { get; internal init; }
    /// <summary>
    /// The consensus timestamp.
    /// </summary>
    public ConsensusTimeStamp? Concensus { get; internal init; }
    /// <summary>
    /// The memo that was submitted with the transaction request.
    /// </summary>
    public string Memo { get; internal init; }
    /// <summary>
    /// The fee that was charged by the network for processing the 
    /// transaction and generating associated receipts and records.
    /// </summary>
    public ulong Fee { get; internal init; }
    /// <summary>
    /// A map of tinybar transfers to and from accounts associated with
    /// the record represented by this transaction.
    /// <see cref="IContext.Payer"/>.
    /// </summary>
    public ReadOnlyDictionary<Address, long> Transfers { get; internal init; }
    /// <summary>
    /// A list of token transfers to and from accounts associated with
    /// the record represented by this transaction.
    /// </summary>
    public ReadOnlyCollection<TokenTransfer> TokenTransfers { get; internal init; }
    /// <summary>
    /// A list of asset transfers to and from accounts associated with
    /// the record represented by this transaction.
    /// </summary>
    public ReadOnlyCollection<AssetTransfer> AssetTransfers { get; internal init; }
    /// <summary>
    /// A list of token transfers applied by the network as royalties
    /// for executing the original transaction.  Typically in the form
    /// of royalties for transfering custom tokens and assets as defined
    /// by the respective token definition's fees.
    /// </summary>
    public ReadOnlyCollection<RoyaltyTransfer> Royalties { get; internal init; }
    /// <summary>
    /// A list of token associations that were created 
    /// as a result of this transaction.
    /// </summary>
    public ReadOnlyCollection<Association> Associations { get; internal init; }

    /// <summary>
    /// If this record represents a child transaction, the consensus timestamp
    /// of the parent transaction to this transaction, otherwise null.
    /// transaction 
    /// </summary>
    public ConsensusTimeStamp? ParentTransactionConcensus { get; internal init; }
    /// <summary>
    /// A List of account staking rewards paid  as a result of this transaction.
    /// </summary>
    public ReadOnlyDictionary<Address, long> StakingRewards { get; internal init; }
    /// <summary>
    /// Internal Constructor of the record.
    /// </summary>
    internal TransactionRecord(NetworkResult result) : base(result)
    {
        var record = result.Record!;
        var (tokenTransfers, assetTransfers) = record.TokenTransferLists.AsTokenAndAssetTransferLists();
        Hash = record.TransactionHash.Memory;
        Concensus = record.ConsensusTimestamp?.ToConsensusTimeStamp();
        Memo = record.Memo;
        Fee = record.TransactionFee;
        Transfers = record.TransferList?.ToTransfers() ?? new ReadOnlyDictionary<Address, long>(new Dictionary<Address, long>());
        TokenTransfers = tokenTransfers;
        AssetTransfers = assetTransfers;
        Royalties = record.AssessedCustomFees.AsRoyaltyTransferList();
        Associations = record.AutomaticTokenAssociations.AsAssociationList();
        ParentTransactionConcensus = record.ParentConsensusTimestamp?.ToConsensusTimeStamp();
        StakingRewards = record.PaidStakingRewards.AsStakingRewards();
    }
}

internal static class TransactionRecordExtensions
{
    private static readonly ReadOnlyCollection<TransactionRecord> EMPTY_RESULT = new List<TransactionRecord>().AsReadOnly();
    internal static ReadOnlyCollection<TransactionRecord> Create(Proto.TransactionRecord? rootRecord, RepeatedField<Proto.TransactionRecord>? childrenRecords, RepeatedField<Proto.TransactionRecord>? failedRecords)
    {
        var count = (rootRecord != null ? 1 : 0) + (childrenRecords != null ? childrenRecords.Count : 0) + (failedRecords != null ? failedRecords.Count : 0);
        if (count > 0)
        {
            var result = new List<TransactionRecord>(count);
            if (rootRecord is not null)
            {
                result.Add(new NetworkResult
                {
                    TransactionID = rootRecord.TransactionID,
                    Receipt = rootRecord.Receipt,
                    Record = rootRecord
                }.ToRecord());
            }
            if (childrenRecords is not null && childrenRecords.Count > 0)
            {
                // The network DOES NOT return the
                // child transaction ID, so we have
                // to synthesize it.
                var nonce = 1;
                foreach (var entry in childrenRecords)
                {
                    var childTransactionId = entry.TransactionID.Clone();
                    childTransactionId.Nonce = nonce;
                    result.Add(new NetworkResult
                    {
                        TransactionID = childTransactionId,
                        Receipt = entry.Receipt,
                        Record = entry
                    }.ToRecord());
                    nonce++;
                }
            }
            if (failedRecords is not null && failedRecords.Count > 0)
            {
                foreach (var entry in failedRecords)
                {
                    result.Add(new NetworkResult
                    {
                        TransactionID = entry.TransactionID,
                        Receipt = entry.Receipt,
                        Record = entry
                    }.ToRecord());
                }
            }
            return result.AsReadOnly();
        }
        return EMPTY_RESULT;
    }

    internal static ReadOnlyDictionary<Address, long> AsStakingRewards(this RepeatedField<AccountAmount> rewards)
    {
        var results = new Dictionary<Address, long>();
        if (rewards is not null)
        {
            foreach (var xfer in rewards)
            {
                var account = xfer.AccountID.AsAddress();
                results.TryGetValue(account, out long amount);
                results[account] = amount + xfer.Amount;
            }
        }
        return new ReadOnlyDictionary<Address, long>(results);
    }
}