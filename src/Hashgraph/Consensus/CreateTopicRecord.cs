#pragma warning disable CS8618 // Non-nullable field is uninitialized.

namespace Hashgraph
{
    /// <summary>
    /// Record produced from creating a new contract.
    /// </summary>
    public sealed class CreateTopicRecord : TransactionRecord
    {
        /// <summary>
        /// The newly created topic instance address.
        /// </summary>
        public Address Topic { get; internal set; }
    }
}