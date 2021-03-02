#pragma warning disable CS8618
using System;

namespace Hashgraph
{
    /// <summary>
    /// The information returned from the GetPendingTransactionInfo
    /// Client  method call.  It represents the details concerning a 
    /// pending (scheduled, not yet executed) transaction held by the
    /// network awaiting signatures.
    /// </summary>
    public sealed record PendingTransactionInfo : PendingTransaction
    {
        /// <summary>
        /// The Account that paid for the scheduling of the 
        /// pending transaction.
        /// </summary>
        public Address Creator { get; internal init; }
        /// <summary>
        /// The account paying for the execution of the
        /// pending transaction.
        /// </summary>
        public Address Payer { get; internal init; }
        /// <summary>
        /// A list of keys having signed the pending transaction, when
        /// all necessary keyholders have signed, the network will attempt
        /// to execute the transaction.
        /// </summary>
        public Endorsement[] Endorsements { get; internal init; }
        /// <summary>
        /// The endorsement key that can cancel this pending transaction.
        /// It may be null, in wich case it can not be canceled and will
        /// exit until signed or expired by the network.
        /// </summary>
        public Endorsement? Administrator { get; internal init; }
        /// <summary>
        /// Optional memo attached to the scheduling of 
        /// the pending transaction.
        /// </summary>
        public string? Memo { get; internal init; }
        /// <summary>
        /// The time at which the pending transaction will expire
        /// and be removed from the network if not signed by 
        /// all necessary parties and executed.
        /// </summary>
        public DateTime Expiration { get; internal init; }
    }
}
