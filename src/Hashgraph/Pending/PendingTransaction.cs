#pragma warning disable CS8618
using System;

namespace Hashgraph
{
    /// <summary>
    /// Information identifying a pending transaction, includes the
    /// address of the pending transaction record, plus the transaction
    /// id that will exist representing the executed transaction if it
    /// is ultimately executed (and not timed out or delted).
    /// </summary>
    public record PendingTransaction
    {
        /// <summary>
        /// The identifier of the pending transaction 
        /// record held by the network.
        /// </summary>
        public Address Id { get; internal init; }
        /// <summary>
        /// The ID of the pending transaction, should it be executed.
        /// </summary>
        public TxId TxId { get; internal init; }
    }
}
