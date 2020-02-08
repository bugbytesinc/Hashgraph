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
        /// Updates the changeable properties of a Hedera Network Topic.
        /// </summary>
        /// <param name="updateParameters">
        /// The Topic update parameters, includes a required 
        /// <see cref="Address"/> reference to the Topic to update plus
        /// a number of changeable properties of the Topic.
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
        public Task<TransactionReceipt> UpdateTopicAsync(UpdateTopicParams updateParameters, Action<IContext>? configure = null)
        {
            return UpdateTopicImplementationAsync<TransactionReceipt>(updateParameters, configure);
        }
        /// <summary>
        /// Updates the changeable properties of a hedera network Topic.
        /// </summary>
        /// <param name="updateParameters">
        /// The Topic update parameters, includes a required 
        /// <see cref="Address"/> reference to the Topic to update plus
        /// a number of changeable properties of the Topic.
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
        public Task<TransactionRecord> UpdateTopicWithRecordAsync(UpdateTopicParams updateParameters, Action<IContext>? configure = null)
        {
            return UpdateTopicImplementationAsync<TransactionRecord>(updateParameters, configure);
        }
        /// <summary>
        /// Internal implementation of the update Topic functionality.
        /// </summary>
        private async Task<TResult> UpdateTopicImplementationAsync<TResult>(UpdateTopicParams updateParameters, Action<IContext>? configure) where TResult : new()
        {
            updateParameters = RequireInputParameter.UpdateParameters(updateParameters);
            await using var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatory = Transactions.GatherSignatories(context, updateParameters.Signatory);
            var updateTopicBody = new ConsensusUpdateTopicTransactionBody
            {
                TopicID = Protobuf.ToTopicID(updateParameters.Topic)
            };
            if (updateParameters.Memo != null)
            {
                updateTopicBody.Memo = updateParameters.Memo;
            }
            if (!(updateParameters.Administrator is null))
            {
                updateTopicBody.AdminKey = Protobuf.ToPublicKey(updateParameters.Administrator);
            }
            if (!(updateParameters.Participant is null))
            {
                updateTopicBody.SubmitKey = Protobuf.ToPublicKey(updateParameters.Participant);
            }
            if (updateParameters.RenewPeriod.HasValue)
            {
                updateTopicBody.AutoRenewPeriod = Protobuf.ToDuration(updateParameters.RenewPeriod.Value);
            }
            if (!(updateParameters.RenewAccount is null))
            {
                updateTopicBody.AutoRenewAccount = Protobuf.ToAccountID(updateParameters.RenewAccount);
            }
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId, "Update Topic");
            transactionBody.ConsensusUpdateTopic = updateTopicBody;
            var request = await Transactions.SignTransactionAsync(transactionBody, signatory);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to update Topic, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
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
                var client = new ConsensusService.ConsensusServiceClient(channel);
                return async (Transaction transaction) => await client.updateTopicAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
