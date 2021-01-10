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
        /// Submits an arbitrary type-un-checked message to the network.
        /// Typically requires a paying account having super-user privilege
        /// access to the network.  The transaction is still signed via the
        /// usual means, but the transaction contents itself must be encoded 
        /// manually by calling code.
        /// </summary>
        /// <param name="transaction">
        /// The serialized protobuf encoded bytes of a Gossip Network Transaction.
        /// These bytes must be manually created from calling code having a knowledge
        /// of how to construct the transaction.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A Submit Message Receipt of the encoded transaction indicating success.  Depending on the contents
        /// of the message, the instance may be a derived class of <code>TransactionReceipt</code>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> SubmitUnsafeTransactionAsync(ReadOnlyMemory<byte> transaction, Action<IContext>? configure = null)
        {
            return SubmitUnsafeTransactionImplementationAsync<TransactionReceipt>(transaction, configure);
        }
        /// <summary>
        /// Submits an arbitrary type-un-checked message to the network.
        /// Typically requires a paying account having super-user privilege
        /// access to the network.  The transaction is still signed via the
        /// usual means, but the transaction contents itself must be encoded 
        /// manually by calling code.
        /// </summary>
        /// <param name="transaction">
        /// The serialized protobuf encoded bytes of a Gossip Network Transaction.
        /// These bytes must be manually created from calling code having a knowledge
        /// of how to construct the transaction.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A Submit Message Record of the encoded transaction indicating success.  Depending on the contents
        /// of the message, the instance may be a derived class of <code>TransactionReceipt</code>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionRecord> SubmitUnsafeTransactionWithRecordAsync(ReadOnlyMemory<byte> transaction, Action<IContext>? configure = null)
        {
            return SubmitUnsafeTransactionImplementationAsync<TransactionRecord>(transaction, configure);
        }
        /// <summary>
        /// Internal implementation of the submit message call.
        /// </summary>
        private async Task<TResult> SubmitUnsafeTransactionImplementationAsync<TResult>(ReadOnlyMemory<byte> transaction, Action<IContext>? configure) where TResult : new()
        {
            var innerTransactionId = RequireInputParameter.IdFromTransactionBytes(transaction);
            await using var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatories = Transactions.GatherSignatories(context);
            var outerTransactionId = Transactions.GetOrCreateTransactionID(context);
            // Note: custom transaction body, does not carry a max fee since 
            // the inner transaction is the transaction to process, it still 
            // must be signed however.
            var transactionBody = new TransactionBody
            {
                TransactionID = outerTransactionId,
                NodeAccountID = new AccountID(RequireInContext.Gateway(context)),
                TransactionValidDuration = new Proto.Duration(context.TransactionDuration),
                UncheckedSubmit = new UncheckedSubmitBody { TransactionBytes = ByteString.CopyFrom(transaction.Span) }
            };
            var precheck = await transactionBody.SignAndSubmitWithRetryAsync(signatories, context);
            ValidateResult.PreCheck(outerTransactionId, precheck);
            // NOTE: The outer transaction ID exists so that the administrative account has something to sign that
            // can be verified, however, the transaction never actually exists in the system so there will never be
            // a receipt for this submission, however, there will be an attempt to execute the submitted transaction
            // as this method bypasses PRECHECK validations.  So, there will be a receipt for the inner traction, with
            // success or a failure code.  Therefore we return the receipt or record for the custom transaction.
            var receipt = await Transactions.GetReceiptAsync(context, innerTransactionId);
            // Retain standard behavior of throwing an exception if the receipt has an error code.
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Submit Unsafe Transaction failed, status: {receipt.Status}", innerTransactionId.ToTxId(), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is TransactionRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, innerTransactionId);
                record.FillProperties(rec);
            }
            else if (result is TransactionReceipt rcpt)
            {
                receipt.FillProperties(innerTransactionId, rcpt);
            }
            return result;
        }
    }
}
