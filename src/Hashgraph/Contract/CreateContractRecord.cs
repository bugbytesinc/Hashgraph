#pragma warning disable CS8618 // Non-nullable field is uninitialized.

namespace Hashgraph
{
    /// <summary>
    /// Record produced from creating a new contract.
    /// </summary>
    public sealed class CreateContractRecord : TransactionRecord
    {
        /// <summary>
        /// The newly created contract instance address.
        /// </summary>
        public Address Contract { get; internal set; }
        /// <summary>
        /// The results returned from the contract create call.
        /// </summary>
        public ContractCallResult CallResult { get; internal set; }
    }
}