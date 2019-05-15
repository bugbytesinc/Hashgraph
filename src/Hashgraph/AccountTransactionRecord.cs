#pragma warning disable CS8618 // Non-nullable field is uninitialized.

namespace Hashgraph
{
    /// <summary>
    /// A transaction record containing information concerning the newly created account.
    /// </summary>
    public sealed class AccountTransactionRecord : TransactionRecord
    {
        /// <summary>
        /// The address of the newly created account.
        /// </summary>
        public Address Address { get; internal set; }
    }
}