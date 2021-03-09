using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Updates the changeable properties of a hedera network account.
        /// </summary>
        /// <param name="updateParameters">
        /// The account update parameters, includes a required 
        /// <see cref="Address"/> reference to the account to update plus
        /// a number of changeable properties of the account.
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
        public async Task<TransactionReceipt> UpdateAccountAsync(UpdateAccountParams updateParameters, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await UpdateAccountImplementationAsync(updateParameters, configure, false));
        }
        /// <summary>
        /// Updates the changeable properties of a hedera network account.
        /// </summary>
        /// <param name="updateParameters">
        /// The account update parameters, includes a required 
        /// <see cref="Address"/> reference to the account to update plus
        /// a number of changeable properties of the account.
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
        public async Task<TransactionRecord> UpdateAccountWithRecordAsync(UpdateAccountParams updateParameters, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await UpdateAccountImplementationAsync(updateParameters, configure, true));
        }
        /// <summary>
        /// Internal implementation of the update account functionality.
        /// </summary>
        private async Task<NetworkResult> UpdateAccountImplementationAsync(UpdateAccountParams updateParameters, Action<IContext>? configure, bool includeRecord)
        {
            updateParameters = RequireInputParameter.UpdateParameters(updateParameters);
            await using var context = CreateChildContext(configure);
            var updateAccountBody = new CryptoUpdateTransactionBody
            {
                AccountIDToUpdate = new AccountID(updateParameters.Address)
            };
            if (!(updateParameters.Endorsement is null))
            {
                updateAccountBody.Key = new Key(updateParameters.Endorsement);
            }
            if (updateParameters.RequireReceiveSignature.HasValue)
            {
                updateAccountBody.ReceiverSigRequiredWrapper = updateParameters.RequireReceiveSignature.Value;
            }
            if (updateParameters.AutoRenewPeriod.HasValue)
            {
                updateAccountBody.AutoRenewPeriod = new Duration(updateParameters.AutoRenewPeriod.Value);
            }
            if (updateParameters.Expiration.HasValue)
            {
                updateAccountBody.ExpirationTime = new Timestamp(updateParameters.Expiration.Value);
            }
            if (!(updateParameters.Proxy is null))
            {
                updateAccountBody.ProxyAccountID = new AccountID(updateParameters.Proxy);
            }
            if (!(updateParameters.Memo is null))
            {
                updateAccountBody.Memo = updateParameters.Memo;
            }
            var transactionBody = new TransactionBody
            {
                CryptoUpdateAccount = updateAccountBody
            };
            return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Unable to update account, status: {0}", updateParameters.Signatory);
        }
    }
}
