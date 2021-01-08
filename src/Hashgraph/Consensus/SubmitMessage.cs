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
        /// A Submit Message Receipt indicating success, includes information
        /// about the sequence number of the message and its running hash.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<SubmitMessageReceipt> SubmitMessageAsync(Address topic, ReadOnlyMemory<byte> message, Action<IContext>? configure = null)
        {
            return SubmitMessageImplementationAsync<SubmitMessageReceipt>(topic, message, false, null, 0, 0, null, configure);
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
        /// A Submit Message Receipt indicating success, includes information
        /// about the sequence number of the message and its running hash.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<SubmitMessageReceipt> SubmitMessageAsync(Address topic, ReadOnlyMemory<byte> message, Signatory signatory, Action<IContext>? configure = null)
        {
            return SubmitMessageImplementationAsync<SubmitMessageReceipt>(topic, message, false, null, 0, 0, signatory, configure);
        }
        /// <summary>
        /// Sends a segment of a message to the network for a given consensus topic.
        /// The caller of this method is responsible for managing the segment of the
        /// message and associated metadata.
        /// </summary>
        /// <param name="submitParams">
        /// Details of the message segment to upload, including the metadata
        /// corresponding to this segment.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A Submit Message Receipt indicating success, includes information
        /// about the sequence number of the message and its running hash.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<SubmitMessageReceipt> SubmitMessageAsync(SubmitMessageParams submitParams, Action<IContext>? configure = null)
        {
            return SubmitMessageImplementationAsync<SubmitMessageReceipt>(submitParams.Topic, submitParams.Segment, true, submitParams.ParentTxId, submitParams.Index, submitParams.TotalSegmentCount, submitParams.Signatory, configure);
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
        /// A Submit Message Receipt indicating success, includes information
        /// about the sequence number of the message and its running hash.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<SubmitMessageRecord> SubmitMessageWithRecordAsync(Address topic, ReadOnlyMemory<byte> message, Action<IContext>? configure = null)
        {
            return SubmitMessageImplementationAsync<SubmitMessageRecord>(topic, message, false, null, 0, 0, null, configure);
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
        /// A Submit Message Record indicating success, includes information
        /// about the sequence number of the message and its running hash.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<SubmitMessageRecord> SubmitMessageWithRecordAsync(Address topic, ReadOnlyMemory<byte> message, Signatory signatory, Action<IContext>? configure = null)
        {
            return SubmitMessageImplementationAsync<SubmitMessageRecord>(topic, message, false, null, 0, 0, signatory, configure);
        }
        /// <summary>
        /// Sends a message to the network for a given consensus topic.
        /// </summary>
        /// <param name="submitParams">
        /// Details of the message segment to upload, including the metadata
        /// corresponding to this segment.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A Submit Message Record indicating success, includes information
        /// about the sequence number of the message and its running hash.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<SubmitMessageRecord> SubmitMessageWithRecordAsync(SubmitMessageParams submitParams, Action<IContext>? configure = null)
        {
            return SubmitMessageImplementationAsync<SubmitMessageRecord>(submitParams.Topic, submitParams.Segment, true, submitParams.ParentTxId, submitParams.Index, submitParams.TotalSegmentCount, submitParams.Signatory, configure);
        }
        /// <summary>
        /// Internal implementation of the submit message call.
        /// </summary>
        private async Task<TResult> SubmitMessageImplementationAsync<TResult>(Address topic, ReadOnlyMemory<byte> message, bool isSegment, TxId? parentTx, int segmentIndex, int segmentTotalCount, Signatory? signatory, Action<IContext>? configure) where TResult : new()
        {
            topic = RequireInputParameter.Topic(topic);
            message = RequireInputParameter.Message(message);
            await using var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatories = Transactions.GatherSignatories(context, signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId);
            transactionBody.ConsensusSubmitMessage = new ConsensusSubmitMessageTransactionBody
            {
                TopicID = new TopicID(topic),
                Message = ByteString.CopyFrom(message.Span),
                ChunkInfo = isSegment ? createChunkInfo(transactionId, parentTx, segmentIndex, segmentTotalCount) : null
            };
            var precheck = await Transactions.SignAndSubmitTransactionWithRetryAsync(transactionBody, signatories, context, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Submit Message failed, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is SubmitMessageRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                record.FillProperties(rec);
            }
            else if (result is SubmitMessageReceipt rcpt)
            {
                receipt.FillProperties(transactionId, rcpt);
            }
            return result;

            static ConsensusMessageChunkInfo createChunkInfo(TransactionID transactionId, TxId? parentTx, int segmentIndex, int segmentTotalCount)
            {
                if (segmentTotalCount < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(SubmitMessageParams.TotalSegmentCount), "Total Segment Count must be a positive number.");
                }
                if (segmentIndex > segmentTotalCount || segmentIndex < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(SubmitMessageParams.Index), "Segment index must be between one and the total segment count inclusively.");
                }
                if (segmentIndex == 1)
                {
                    if (!(parentTx is null))
                    {
                        throw new ArgumentOutOfRangeException(nameof(SubmitMessageParams.ParentTxId), "The Parent Transaction cannot be specified (must be null) when the segment index is one.");
                    }
                    return new ConsensusMessageChunkInfo
                    {
                        Total = segmentTotalCount,
                        Number = segmentIndex,
                        InitialTransactionID = transactionId
                    };
                }
                if (parentTx is null)
                {
                    throw new ArgumentNullException(nameof(SubmitMessageParams.ParentTxId), "The parent transaction id is required when segment index is greater than one.");
                }
                return new ConsensusMessageChunkInfo
                {
                    Total = segmentTotalCount,
                    Number = segmentIndex,
                    InitialTransactionID = new TransactionID(parentTx)
                };
            }

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
