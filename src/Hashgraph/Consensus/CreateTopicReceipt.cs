#pragma warning disable CS8618 // Non-nullable field is uninitialized.

namespace Hashgraph
{
    /// <summary>
    /// Receipt produced from creating a new contract.
    /// </summary>
    public sealed class CreateTopicReceipt : TransactionReceipt
    {
        /// <summary>
        /// The newly created or associated topic instance address.
        /// </summary>
        public Address Topic { get; internal set; }
    }
}