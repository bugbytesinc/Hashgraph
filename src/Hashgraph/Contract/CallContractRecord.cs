using Hashgraph.Implementation;

namespace Hashgraph
{
    /// <summary>
    /// Record produced from creating a new contract.
    /// </summary>
    public sealed record CallContractRecord : TransactionRecord
    {
        /// <summary>
        /// The results returned from the contract call.
        /// </summary>
        public ContractCallResult CallResult { get; internal init; }
        /// <summary>
        /// Internal Constructor of the record.
        /// </summary>
        internal CallContractRecord(NetworkResult result) : base(result)
        {
            CallResult = result.Record!.ContractCallResult.ToContractCallResult();
        }
    }
}