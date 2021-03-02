using Hashgraph.Implementation;
using Proto;
using System;
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
        /// Internal Constructor of the record.
        /// </summary>
        internal TransactionRecord(NetworkResult result) : base(result)
        {
            var record = result.Record!;
            Hash = record.TransactionHash?.ToByteArray();
            Concensus = record.ConsensusTimestamp?.ToDateTime();
            Memo = record.Memo;
            Fee = record.TransactionFee;
            Transfers = record.TransferList.ToTransfers();
            TokenTransfers = record.TokenTransferLists.ToTransfers();
        }
    }
}
