using Hashgraph.Implementation;

namespace Hashgraph
{
    /// <summary>
    /// Record produced from creating a new contract.
    /// </summary>
    public sealed record CreateTopicRecord : TransactionRecord
    {
        /// <summary>
        /// The newly created topic instance address.
        /// </summary>
        /// <remarks>
        /// The value will be <code>None</code> if the create acocunt
        /// method was scheduled as a pending transaction.
        /// </remarks>
        public Address Topic { get; internal init; }
        /// <summary>
        /// Internal Constructor of the record.
        /// </summary>
        internal CreateTopicRecord(NetworkResult result) : base(result)
        {
            Topic = result.Receipt.TopicID?.ToAddress() ?? Address.None;
        }
    }
}