#pragma warning disable CS8618 // Non-nullable field is uninitialized.

namespace Hashgraph
{
    /// <summary>
    /// Receipt produced from creating a new contract.
    /// </summary>
    public sealed class ContractReceipt : TransactionReceipt
    {
        /// <summary>
        /// The newly created or associated contract instance address.
        /// </summary>
        public Address Contract { get; internal set; }
    }
}