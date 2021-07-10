using Google.Protobuf.Collections;
using Hashgraph.Implementation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hashgraph
{
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
        public DateTime? Concensus { get; internal init; }
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
        /// A list token transfers to and from accounts associated with
        /// the record represented by this transaction.
        /// </summary>
        public ReadOnlyCollection<TokenTransfer> TokenTransfers { get; internal init; }
        /// <summary>
        /// A list of token transfers applied by the network as commissions
        /// for executing the original transaction.  Typically in the form
        /// of royalties for transfering custom tokens and assets as defined
        /// by the respective token definition's fees.
        /// </summary>
        public ReadOnlyCollection<TokenTransfer> Commissions { get; internal init; }
        /// <summary>
        /// Internal Constructor of the record.
        /// </summary>
        internal TransactionRecord(NetworkResult result) : base(result)
        {
            var record = result.Record!;
            Hash = record.TransactionHash?.ToByteArray();
            Concensus = record.ConsensusTimestamp?.ToDateTime();
            Memo = record.Memo;
            Fee = record.TransactionFee;
            Transfers = record.TransferList?.ToTransfers() ?? new ReadOnlyDictionary<Address, long>(new Dictionary<Address, long>());
            TokenTransfers = TokenTransferExtensions.Create(record.TokenTransferLists);
            Commissions = TokenTransferExtensions.Create(record.AssessedCustomFees);
        }
    }

    internal static class TransactionRecordExtensions
    {
        private static ReadOnlyCollection<TransactionRecord> EMPTY_RESULT = new List<TransactionRecord>().AsReadOnly();
        internal static ReadOnlyCollection<TransactionRecord> Create(RepeatedField<Proto.TransactionRecord> list, Proto.TransactionRecord? first)
        {
            var count = (first != null ? 1 : 0) + (list != null ? list.Count : 0);
            if (count > 0)
            {
                var result = new List<TransactionRecord>(count);
                if (first != null)
                {
                    result.Add(new NetworkResult
                    {
                        TransactionID = first.TransactionID,
                        Receipt = first.Receipt,
                        Record = first
                    }.ToRecord());
                }
                if (list != null && list.Count > 0)
                {
                    foreach (var entry in list)
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
    }
}
