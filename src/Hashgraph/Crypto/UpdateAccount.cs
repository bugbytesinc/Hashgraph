using Grpc.Core;
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
        public Task<TransactionReceipt> UpdateAccountAsync(UpdateAccountParams updateParameters, Action<IContext>? configure = null)
        {
            return UpdateAccountImplementationAsync<TransactionReceipt>(updateParameters, configure);
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
        public Task<TransactionRecord> UpdateAccountWithRecordAsync(UpdateAccountParams updateParameters, Action<IContext>? configure = null)
        {
            return UpdateAccountImplementationAsync<TransactionRecord>(updateParameters, configure);
        }
        /// <summary>
        /// Internal implementation of the update account functionality.
        /// </summary>
        private async Task<TResult> UpdateAccountImplementationAsync<TResult>(UpdateAccountParams updateParameters, Action<IContext>? configure) where TResult : new()
        {
            updateParameters = RequireInputParameter.UpdateParameters(updateParameters);
            await using var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
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
            var signatory = Transactions.GatherSignatories(context, updateParameters.Signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId);
            transactionBody.CryptoUpdateAccount = updateAccountBody;
            var precheck = await Transactions.SignAndSubmitTransactionWithRetryAsync(transactionBody, signatory, context, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to update account, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is TransactionRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                record.FillProperties(rec);
            }
            else if (result is TransactionReceipt rcpt)
            {
                receipt.FillProperties(transactionId, rcpt);
            }
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Transaction transaction) => await client.updateAccountAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
