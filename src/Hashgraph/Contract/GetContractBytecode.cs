using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Retrieves the bytecode for the specified contract.
        /// </summary>
        /// <param name="contract">
        /// The Hedera Network Address of the Contract.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// The bytecode for the specified contract instance.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        public async Task<ReadOnlyMemory<byte>> GetContractBytecodeAsync(Address contract, Action<IContext>? configure = null)
        {
            return (await ExecuteQueryAsync(new ContractGetBytecodeQuery(contract), configure).ConfigureAwait(false)).ContractGetBytecodeResponse.Bytecode.Memory;
        }
    }
}
