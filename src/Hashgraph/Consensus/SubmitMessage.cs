using Google.Protobuf;
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
        /// Sends a message to the network for a given consensus topic.
        /// </summary>
        /// <param name="topic">
        /// The address of the topic for the message.
        /// </param>
        /// <param name="message">
        /// The value of the message, limited to the 4K total Network Transaction Size.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A contract transaction receipt indicating success, it does not
        /// include any output parameters sent from the contract.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<SubmitMessageReceipt> SubmitMessageAsync(Address topic, ReadOnlyMemory<byte> message, Action<IContext>? configure = null)
        {
            return SubmitMessageImplementationAsync<SubmitMessageReceipt>(topic, message, null, configure);
        }
        /// <summary>
        /// Sends a message to the network for a given consensus topic.
        /// </summary>
        /// <param name="topic">
        /// The address of the topic for the message.
        /// </param>
        /// <param name="message">
        /// The value of the message, limited to the 4K total Network Transaction Size.
        /// </param>
        /// <param name="signatory">
        /// The signatory containing any additional private keys or callbacks
        /// to meet the key signing requirements for participants.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A contract transaction receipt indicating success, it does not
        /// include any output parameters sent from the contract.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<SubmitMessageReceipt> SubmitMessageAsync(Address topic, ReadOnlyMemory<byte> message, Signatory signatory, Action<IContext>? configure = null)
        {
            return SubmitMessageImplementationAsync<SubmitMessageReceipt>(topic, message, signatory, configure);
        }
        /// <summary>
        /// Sends a message to the network for a given consensus topic.
        /// </summary>
        /// <param name="topic">
        /// The address of the topic for the message.
        /// </param>
        /// <param name="message">
        /// The value of the message, limited to the 4K total Network Transaction Size.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating success or failure.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<SubmitMessageRecord> SubmitMessageWithRecordAsync(Address topic, ReadOnlyMemory<byte> message, Action<IContext>? configure = null)
        {
            return SubmitMessageImplementationAsync<SubmitMessageRecord>(topic, message, null, configure);
        }
        /// <summary>
        /// Sends a message to the network for a given consensus topic.
        /// </summary>
        /// <param name="topic">
        /// The address of the topic for the message.
        /// </param>
        /// <param name="message">
        /// The value of the message, limited to the 4K total Network Transaction Size.
        /// </param>
        /// <param name="signatory">
        /// The signatory containing any additional private keys or callbacks
        /// to meet the key signing requirements for participants.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating success or failure.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<SubmitMessageRecord> SubmitMessageWithRecordAsync(Address topic, ReadOnlyMemory<byte> message, Signatory signatory, Action<IContext>? configure = null)
        {
            return SubmitMessageImplementationAsync<SubmitMessageRecord>(topic, message, signatory, configure);
        }
        /// <summary>
        /// Internal implementation of the contract call method.
        /// </summary>
        private async Task<TResult> SubmitMessageImplementationAsync<TResult>(Address topic, ReadOnlyMemory<byte> message, Signatory? signatory, Action<IContext>? configure) where TResult : new()
        {
            topic = RequireInputParameter.Topic(topic);
            message = RequireInputParameter.Message(message);
            await using var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatories = Transactions.GatherSignatories(context, signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId, "Submit Message");
            transactionBody.ConsensusSubmitMessage = new ConsensusSubmitMessageTransactionBody
            {
                TopicID = Protobuf.ToTopicID(topic),
                Message = ByteString.CopyFrom(message.Span)
            };
            var request = await Transactions.SignTransactionAsync(transactionBody, signatories);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Submit Message failed, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is SubmitMessageRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);                
                Protobuf.FillRecordProperties(record, rec);
                rec.RunningHash = receipt.TopicRunningHash?.ToByteArray();
                rec.SequenceNumber = receipt.TopicSequenceNumber;
            }
            else if (result is SubmitMessageReceipt rcpt)
            {
                Protobuf.FillReceiptProperties(transactionId, receipt, rcpt);
                rcpt.RunningHash = receipt.TopicRunningHash?.ToByteArray();
                rcpt.SequenceNumber = receipt.TopicSequenceNumber;
            }
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new ConsensusService.ConsensusServiceClient(channel);
                return async (Transaction transaction) => await client.submitMessageAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
