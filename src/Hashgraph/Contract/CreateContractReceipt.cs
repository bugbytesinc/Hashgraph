using Hashgraph.Implementation;

namespace Hashgraph
{
    /// <summary>
    /// Receipt produced from creating a new contract.
    /// </summary>
    public sealed record CreateContractReceipt : TransactionReceipt
    {
        /// <summary>
        /// The newly created or associated contract instance address.
        /// </summary>
        /// <remarks>
        /// The value will be <code>None</code> if the create contract
        /// method was scheduled as a pending transaction.
        /// </remarks>
        public Address Contract { get; internal init; }
        /// <summary>
        /// Internal Constructor of the receipt.
        /// </summary>
        internal CreateContractReceipt(NetworkResult result) : base(result)
        {
            Contract = result.Receipt.ContractID?.ToAddress() ?? Address.None;
        }
    }
}