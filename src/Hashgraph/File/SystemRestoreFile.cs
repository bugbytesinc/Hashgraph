using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Undeletes a file from the network via Administrative Restore
        /// </summary>
        /// <param name="fileToRestore">
        /// The address of the file to restore.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating success of the file deletion.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> SystemRestoreFileAsync(Address fileToRestore, Action<IContext>? configure = null)
        {
            return SystemRestoreFileImplementationAsync<TransactionReceipt>(fileToRestore, null, configure);
        }
        /// <summary>
        /// Undeletes a file from the network via Administrative Restore
        /// </summary>
        /// <param name="fileToRestore">
        /// The address of the file to restore.
        /// </param>
        /// <param name="signatory">
        /// Typically private key, keys or signing callback method that 
        /// are needed to delete the file as per the requirements in the
        /// associated Endorsement.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating success of the file deletion.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> SystemRestoreFileAsync(Address fileToRestore, Signatory signatory, Action<IContext>? configure = null)
        {
            return SystemRestoreFileImplementationAsync<TransactionReceipt>(fileToRestore, signatory, configure);
        }
        /// <summary>
        /// Undeletes a file from the network via Administrative Restore
        /// </summary>
        /// <param name="fileToRestore">
        /// The address of the file to restore.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating success of the file deletion,
        /// fees & other transaction details.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionRecord> SystemRestoreFileWithRecordAsync(Address fileToRestore, Action<IContext>? configure = null)
        {
            return SystemRestoreFileImplementationAsync<TransactionRecord>(fileToRestore, null, configure);
        }
        /// <summary>
        /// Undeletes a file from the network via Administrative Restore
        /// </summary>
        /// <param name="fileToRestore">
        /// The address of the file to restore.
        /// </param>
        /// <param name="signatory">
        /// Typically private key, keys or signing callback method that 
        /// are needed to delete the file as per the requirements in the
        /// associated Endorsement.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating success of the file deletion,
        /// fees & other transaction details.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionRecord> SystemRestoreFileWithRecordAsync(Address fileToRestore, Signatory signatory, Action<IContext>? configure = null)
        {
            return SystemRestoreFileImplementationAsync<TransactionRecord>(fileToRestore, signatory, configure);
        }
        /// <summary>
        /// Internal helper function implementing the file delete functionality.
        /// </summary>
        public async Task<TResult> SystemRestoreFileImplementationAsync<TResult>(Address fileToRestore, Signatory? signatory, Action<IContext>? configure = null) where TResult : new()
        {
            fileToRestore = RequireInputParameter.FileToRestore(fileToRestore);
            await using var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatories = Transactions.GatherSignatories(context, signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = new TransactionBody(context, transactionId);
            transactionBody.SystemUndelete = new SystemUndeleteTransactionBody
            {
                FileID = new FileID(fileToRestore)
            };
            var receipt = await transactionBody.SignAndExecuteWithRetryAsync(signatories, context);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to delete file, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
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
