using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Restores a contract to the network via Administrative Restore
        /// </summary>
        /// <param name="contractToRestore">
        /// The address of the contract to restore.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating success of the contract deletion.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> SystemRestoreContractAsync(Address contractToRestore, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await SystemRestoreContractImplementationAsync(contractToRestore, null, configure, false));
        }
        /// <summary>
        /// Restores a contract to the network via Administrative Restore
        /// </summary>
        /// <param name="contractToRestore">
        /// The address of the contract to restore.
        /// </param>
        /// <param name="signatory">
        /// Typically private key, keys or signing callback method that 
        /// are needed to delete the contract as per the requirements in the
        /// associated Endorsement.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating success of the contract deletion.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> SytemRestoreContractAsync(Address contractToRestore, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await SystemRestoreContractImplementationAsync(contractToRestore, signatory, configure, false));
        }
        /// <summary>
        /// Restores a contract to the network via Administrative Restore
        /// </summary>
        /// <param name="contractToRestore">
        /// The address of the contract to restore.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating success of the contract deletion,
        /// fees & other transaction details.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionRecord> SystemRestoreContractWithRecordAsync(Address contractToRestore, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await SystemRestoreContractImplementationAsync(contractToRestore, null, configure, true));
        }
        /// <summary>
        /// Restores a contract to the network via Administrative Restore
        /// </summary>
        /// <param name="contractToRestore">
        /// The address of the contract to restore.
        /// </param>
        /// <param name="signatory">
        /// Typically private key, keys or signing callback method that 
        /// are needed to delete the contract as per the requirements in the
        /// associated Endorsement.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating success of the contract deletion,
        /// fees & other transaction details.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionRecord> SystemRestoreContractWithRecordAsync(Address contractToRestore, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await SystemRestoreContractImplementationAsync(contractToRestore, signatory, configure, true));
        }
        /// <summary>
        /// Internal helper function implementing the contract delete functionality.
        /// </summary>
        private async Task<NetworkResult> SystemRestoreContractImplementationAsync(Address contractToRestore, Signatory? signatory, Action<IContext>? configure, bool includeRecord)
        {
            contractToRestore = RequireInputParameter.ContractToRestore(contractToRestore);
            await using var context = CreateChildContext(configure);
            var transactionBody = new TransactionBody
            {
                SystemUndelete = new SystemUndeleteTransactionBody
                {
                    ContractID = new ContractID(contractToRestore)
                }
            };
            return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Unable to restore contract, status: {0}", signatory);
        }
    }
}
