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
        /// Creates a new file with the given content.
        /// </summary>
        /// <param name="createParameters">
        /// File creation parameters specifying contents and ownership of the file.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt with a description of the newly created file.
        /// and record information.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<FileReceipt> CreateFileAsync(CreateFileParams createParameters, Action<IContext>? configure = null)
        {
            return CreateFileImplementationAsync<FileReceipt>(createParameters, configure);
        }
        /// <summary>
        /// Creates a new file with the given content.
        /// </summary>
        /// <param name="createParameters">
        /// File creation parameters specifying contents and ownership of the file.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record with a description of the newly created file & fees.
        /// and record information.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<FileRecord> CreateFileWithRecordAsync(CreateFileParams createParameters, Action<IContext>? configure = null)
        {
            return CreateFileImplementationAsync<FileRecord>(createParameters, configure);
        }
        /// <summary>
        /// Internal implementation of the Create File service.
        /// </summary>
        public async Task<TResult> CreateFileImplementationAsync<TResult>(CreateFileParams createParameters, Action<IContext>? configure) where TResult : new()
        {
            createParameters = RequireInputParameter.CreateParameters(createParameters);
            var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateEmptyTransactionBody(context, transactionId, "Create File");
            transactionBody.FileCreate = new FileCreateTransactionBody
            {
                ExpirationTime = Protobuf.ToTimestamp(createParameters.Expiration),
                Keys = Protobuf.ToPublicKeyList(createParameters.Endorsements),
                Contents = ByteString.CopyFrom(createParameters.Contents.ToArray()),
            };
            var request = Transactions.SignTransaction(transactionBody, payer);
            var precheck = await Transactions.ExecuteRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck.NodeTransactionPrecheckCode);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to create file, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is FileRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                Protobuf.FillRecordProperties(transactionId, record, rec);
                rec.File = Protobuf.FromFileID(receipt.FileID);
            }
            else if (result is FileReceipt rcpt)
            {
                Protobuf.FillReceiptProperties(transactionId, receipt, rcpt);
                rcpt.File = Protobuf.FromFileID(receipt.FileID);
            }
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new FileService.FileServiceClient(channel);
                return async (Transaction transaction) => await client.createFileAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
