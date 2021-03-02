using Hashgraph.Implementation;

namespace Hashgraph
{
    /// <summary>
    /// A transaction record containing information regarding
    /// new token coin balance, typically returned from methods
    /// that can affect a change on the total circulation supply.
    /// </summary>
    public sealed record TokenRecord : TransactionRecord
    {
        /// <summary>
        /// The current (new) total balance of tokens 
        /// in all accounts (the whole denomination).
        /// </summary>
        /// <remarks>
        /// The value will be <code>0</code> if the update
        /// was scheduled as a pending transaction.
        /// </remarks>
        public ulong Circulation { get; internal init; }
        /// <summary>
        /// Internal Constructor of the record.
        /// </summary>
        internal TokenRecord(NetworkResult result) : base(result)
        {
            Circulation = result.Record!.Receipt.NewTotalSupply;
        }
    }
}