using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Updates the changeable properties of a hedera network contract.
        /// </summary>
        /// <param name="updateParameters">
        /// The contract update parameters, includes a required 
        /// <see cref="Address"/> reference to the Contract to update plus
        /// a number of changeable properties of the Contract.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating success of the operation.
        /// of the request.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> UpdateContractAsync(UpdateContractParams updateParameters, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await UpdateContractImplementationAsync(updateParameters, configure, false));
        }
        /// <summary>
        /// Updates the changeable properties of a hedera network Contract.
        /// </summary>
        /// <param name="updateParameters">
        /// The Contract update parameters, includes a required 
        /// <see cref="Address"/> reference to the Contract to update plus
        /// a number of changeable properties of the Contract.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record containing the details of the results.
        /// of the request.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionRecord> UpdateContractWithRecordAsync(UpdateContractParams updateParameters, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await UpdateContractImplementationAsync(updateParameters, configure, true));
        }
        /// <summary>
        /// Internal implementation of the update Contract functionality.
        /// </summary>
        private async Task<NetworkResult> UpdateContractImplementationAsync(UpdateContractParams updateParameters, Action<IContext>? configure, bool includeRecord)
        {
            updateParameters = RequireInputParameter.UpdateParameters(updateParameters);
            await using var context = CreateChildContext(configure);
            var updateContractBody = new ContractUpdateTransactionBody
            {
                ContractID = new ContractID(updateParameters.Contract)
            };
            if (updateParameters.Expiration.HasValue)
            {
                updateContractBody.ExpirationTime = new Timestamp(updateParameters.Expiration.Value);
            }
            if (!(updateParameters.Administrator is null))
            {
                updateContractBody.AdminKey = new Key(updateParameters.Administrator);
            }
            if (updateParameters.RenewPeriod.HasValue)
            {
                updateContractBody.AutoRenewPeriod = new Duration(updateParameters.RenewPeriod.Value);
            }
            if (!(updateParameters.File is null))
            {
                updateContractBody.FileID = new FileID(updateParameters.File);
            }
            if (!string.IsNullOrWhiteSpace(updateParameters.Memo))
            {
                updateContractBody.Memo = updateParameters.Memo;
            }
            var transactionBody = new TransactionBody
            {
                ContractUpdateInstance = updateContractBody
            };
            return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Unable to update Contract, status: {0}", updateParameters.Signatory);
        }
    }
}
