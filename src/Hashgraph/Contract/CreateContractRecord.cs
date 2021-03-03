using Hashgraph.Implementation;
using Proto;

namespace Hashgraph
{
    /// <summary>
    /// Record produced from creating a new contract.
    /// </summary>
    public sealed record CreateContractRecord : TransactionRecord
    {
        /// <summary>
        /// The newly created contract instance address.
        /// </summary>
        /// <remarks>
        /// The value will be <code>None</code> if the create contract
        /// request was scheduled as a pending transaction.
        /// </remarks>
        public Address Contract { get; internal init; }
        /// <summary>
        /// The results returned from the contract create call.
        /// </summary>
        public ContractCallResult CallResult { get; internal init; }
        /// <summary>
        /// Internal Constructor of the record.
        /// </summary>
        internal CreateContractRecord(NetworkResult result) : base(result)
        {

            Contract = result.Receipt.ContractID.AsAddress();
            CallResult = result.Record!.ContractCreateResult.ToContractCallResult();
        }
    }
}