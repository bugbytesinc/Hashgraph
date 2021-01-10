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
        /// Updates the properties or contents of an existing file stored in the network.
        /// </summary>
        /// <param name="updateParameters">
        /// Update parameters indicating the file to update and what properties such 
        /// as the access key or content that should be updated.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating the operation was successful.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> UpdateFileAsync(UpdateFileParams updateParameters, Action<IContext>? configure = null)
        {
            return UpdateFileImplementationAsync<TransactionReceipt>(updateParameters, configure);
        }
        /// <summary>
        /// Updates the properties or contents of an existing file stored in the network.
        /// </summary>
        /// <param name="updateParameters">
        /// Update parameters indicating the file to update and what properties such 
        /// as the access key or content that should be updated.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record describing the details of the operation 
        /// including fees and transaction hash.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionRecord> UpdateFileWithRecordAsync(UpdateFileParams updateParameters, Action<IContext>? configure = null)
        {
            return UpdateFileImplementationAsync<TransactionRecord>(updateParameters, configure);
        }
        /// <summary>
        /// Internal helper method implementing the file update service.
        /// </summary>
        public async Task<TResult> UpdateFileImplementationAsync<TResult>(UpdateFileParams updateParameters, Action<IContext>? configure) where TResult : new()
        {
            updateParameters = RequireInputParameter.UpdateParameters(updateParameters);
            await using var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatory = Transactions.GatherSignatories(context, updateParameters.Signatory);
            var updateFileBody = new FileUpdateTransactionBody
            {
                FileID = new FileID(updateParameters.File)
            };
            if (!(updateParameters.Endorsements is null))
            {
                updateFileBody.Keys = new KeyList(updateParameters.Endorsements);
            }
            if (updateParameters.Contents.HasValue)
            {
                updateFileBody.Contents = ByteString.CopyFrom(updateParameters.Contents.Value.ToArray());
            }
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = new TransactionBody(context, transactionId);
            transactionBody.FileUpdate = updateFileBody;
            var receipt = await transactionBody.SignAndExecuteWithRetryAsync(signatory, context);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to update file, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
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
        }
    }
}
