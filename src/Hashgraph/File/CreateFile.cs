using Google.Protobuf;
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
            await using var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatory = Transactions.GatherSignatories(context, createParameters.Signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = new TransactionBody(context, transactionId);
            transactionBody.FileCreate = new FileCreateTransactionBody
            {
                ExpirationTime = new Timestamp(createParameters.Expiration),
                Keys = new KeyList(createParameters.Endorsements),
                Contents = ByteString.CopyFrom(createParameters.Contents.ToArray()),
            };
            var receipt = await transactionBody.SignAndExecuteWithRetryAsync(signatory, context);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to create file, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is FileRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                record.FillProperties(rec);
            }
            else if (result is FileReceipt rcpt)
            {
                receipt.FillProperties(transactionId, rcpt);
            }
            return result;
        }
    }
}
