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
            var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var updateAccountBody = new CryptoUpdateTransactionBody
            {
                AccountIDToUpdate = Protobuf.ToAccountID(updateParameters.Address)
            };
            if (!(updateParameters.Endorsement is null))
            {
                updateAccountBody.Key = Protobuf.ToPublicKey(updateParameters.Endorsement);
            }
            if (updateParameters.SendThresholdCreateRecord.HasValue)
            {
                updateAccountBody.SendRecordThresholdWrapper = updateParameters.SendThresholdCreateRecord.Value;
            }
            if (updateParameters.ReceiveThresholdCreateRecord.HasValue)
            {
                updateAccountBody.ReceiveRecordThresholdWrapper = updateParameters.ReceiveThresholdCreateRecord.Value;
            }
            if (updateParameters.AutoRenewPeriod.HasValue)
            {
                updateAccountBody.AutoRenewPeriod = Protobuf.ToDuration(updateParameters.AutoRenewPeriod.Value);
            }
            if (updateParameters.Expiration.HasValue)
            {
                updateAccountBody.ExpirationTime = Protobuf.ToTimestamp(updateParameters.Expiration.Value);
            }
            if (!(updateParameters.Proxy is null))
            {
                updateAccountBody.ProxyAccountID = Protobuf.ToAccountID(updateParameters.Proxy);
            }
            var signatory = Transactions.GatherSignatories(context, updateParameters.Signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId, "Update Account");
            transactionBody.CryptoUpdateAccount = updateAccountBody;
            var request = await Transactions.SignTransactionAsync(transactionBody, signatory);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck.NodeTransactionPrecheckCode);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to update account, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is TransactionRecord arec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                Protobuf.FillRecordProperties(record, arec);
            }
            else if (result is TransactionReceipt arcpt)
            {
                Protobuf.FillReceiptProperties(transactionId, receipt, arcpt);
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
