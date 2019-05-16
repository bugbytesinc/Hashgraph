#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// The details returned from the network after consensus 
    /// has been reached for a network request.
    /// </summary>
    public class TransactionRecord : TransactionReceipt
    {
        /// <summary>
        /// Hash of the Transaction.
        /// </summary>
        public ReadOnlyMemory<byte> Hash { get; internal set; }
        /// <summary>
        /// The consensus timestamp.
        /// </summary>
        public DateTime? Concensus { get; internal set; }
        /// <summary>
        /// The memo that was submitted with the transaction request.
        /// </summary>
        public string Memo { get; internal set; }
        /// <summary>
        /// The fee that was charged by the network for processing the 
        /// transaction and generating associated receipts and records.
        /// </summary>
        public ulong Fee { get; internal set; }
    }
}
