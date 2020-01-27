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
        /// Creates a new topic instance with the given create parameters.
        /// </summary>
        /// <param name="createParameters">
        /// Details regarding the topic to instantiate.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt with a description of the newly created topic.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<CreateTopicReceipt> CreateTopicAsync(CreateTopicParams createParameters, Action<IContext>? configure = null)
        {
            return CreateTopicImplementationAsync<CreateTopicReceipt>(createParameters, configure);
        }
        /// <summary>
        /// Creates a new topic instance with the given create parameters.
        /// </summary>
        /// <param name="createParameters">
        /// Details regarding the topic to instantiate.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record with a description of the newly created topic.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<CreateTopicRecord> CreateTopicWithRecordAsync(CreateTopicParams createParameters, Action<IContext>? configure = null)
        {
            return CreateTopicImplementationAsync<CreateTopicRecord>(createParameters, configure);
        }
        /// <summary>
        /// Internal implementation of the Create ConsensusTopic service.
        /// </summary>
        private async Task<TResult> CreateTopicImplementationAsync<TResult>(CreateTopicParams createParameters, Action<IContext>? configure) where TResult : new()
        {
            createParameters = RequireInputParameter.CreateParameters(createParameters);
            var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatory = Transactions.GatherSignatories(context, createParameters.Signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId, "Create Consensus Topic");
            transactionBody.ConsensusCreateTopic = new ConsensusCreateTopicTransactionBody
            {
                Memo = createParameters.Memo,
                AdminKey = createParameters.Administrator is null ? null : Protobuf.ToPublicKey(createParameters.Administrator),
                SubmitKey = createParameters.Participant is null ? null : Protobuf.ToPublicKey(createParameters.Participant),
                AutoRenewPeriod = Protobuf.ToDuration(createParameters.RenewPeriod),
                AutoRenewAccount = createParameters.RenewAccount is null ? null : Protobuf.ToAccountID(createParameters.RenewAccount)
            };
            var request = await Transactions.SignTransactionAsync(transactionBody, signatory);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to create Consensus Topic, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is CreateTopicRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                Protobuf.FillRecordProperties(record, rec);
                rec.Topic = Protobuf.FromTopicID(receipt.TopicID);
            }
            else if (result is CreateTopicReceipt rcpt)
            {
                Protobuf.FillReceiptProperties(transactionId, receipt, rcpt);
                rcpt.Topic = Protobuf.FromTopicID(receipt.TopicID);
            }
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new ConsensusService.ConsensusServiceClient(channel);
                return async (Transaction transaction) => await client.createTopicAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
