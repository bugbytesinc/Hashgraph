using Hashgraph.Implementation;
using Proto;

namespace Hashgraph
{
    /// <summary>
    /// A transaction receipt containing information concerning the newly created file.
    /// </summary>
    public sealed record FileReceipt : TransactionReceipt
    {
        /// <summary>
        /// The address of the newly created file.
        /// </summary>
        /// <remarks>
        /// The value will be <code>None</code> if the create file
        /// method was scheduled as a pending transaction.
        /// </remarks>
        public Address File { get; internal init; }
        /// <summary>
        /// Internal Constructor of the receipt.
        /// </summary>
        internal FileReceipt(NetworkResult result) : base(result)
        {
            File = result.Receipt.FileID.AsAddress();
        }
    }
}