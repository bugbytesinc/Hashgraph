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
        public async Task<TransactionReceipt> SubmitUnsafeTransactionAsync(ReadOnlyMemory<byte> transaction, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await SubmitUnsafeTransactionImplementationAsync(transaction, configure, false).ConfigureAwait(false));
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
        public async Task<TransactionRecord> SubmitUnsafeTransactionWithRecordAsync(ReadOnlyMemory<byte> transaction, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await SubmitUnsafeTransactionImplementationAsync(transaction, configure, true).ConfigureAwait(false));
        }
        /// <summary>
        /// Internal implementation of the submit message call.
        /// </summary>
        private async Task<NetworkResult> SubmitUnsafeTransactionImplementationAsync(ReadOnlyMemory<byte> transaction, Action<IContext>? configure, bool includeRecord)
        {
            var result = new NetworkResult();
            var innerTransactionId = result.TransactionID = idFromTransactionBytes(transaction);
            await using var context = CreateChildContext(configure);
            var gateway = context.Gateway;
            if (gateway is null)
            {
                throw new InvalidOperationException("The Network Gateway Node has not been configured. Please check that 'Gateway' is set in the Client context.");
            }
            var signatories = context.GatherSignatories();
            var outerTransactionId = context.GetOrCreateTransactionID();
            var uncheckedSubmitBody = new UncheckedSubmitBody(transaction);
            // Note: custom transaction body, does not carry a max fee since 
            // the inner transaction is the transaction to process, it still 
            // must be signed however.
            var transactionBody = new TransactionBody
            {
                TransactionID = outerTransactionId,
                NodeAccountID = new AccountID(gateway),
                TransactionValidDuration = new Proto.Duration(context.TransactionDuration),
                UncheckedSubmit = uncheckedSubmitBody
            };
            var invoice = new Invoice(transactionBody);
            await signatories.SignAsync(invoice).ConfigureAwait(false);
            var signedTransaction = new Transaction
            {
                SignedTransactionBytes = invoice.GenerateSignedTransactionFromSignatures(context.SignaturePrefixTrimLimit).ToByteString()
            };
            var precheck = await context.ExecuteSignedRequestWithRetryImplementationAsync(signedTransaction, (uncheckedSubmitBody as INetworkTransaction).InstantiateNetworkRequestMethod, getResponseCode).ConfigureAwait(false);
            if (precheck.NodeTransactionPrecheckCode != ResponseCodeEnum.Ok)
            {
                var responseCode = (ResponseCode)precheck.NodeTransactionPrecheckCode;
                throw new PrecheckException($"Transaction Failed Pre-Check: {responseCode}", transactionBody.TransactionID.AsTxId(), responseCode, precheck.Cost);
            }
            // NOTE: The outer transaction ID exists so that the administrative account has something to sign that
            // can be verified, however, the transaction never actually exists in the system so there will never be
            // a receipt for this submission, however, there will be an attempt to execute the submitted transaction
            // as this method bypasses PRECHECK validations.  So, there will be a receipt for the inner transaction, 
            // with success or a failure code.  Therefore we return the receipt or record for the custom transaction.
            var receipt = result.Receipt = await context.GetReceiptAsync(innerTransactionId).ConfigureAwait(false);
            // Retain standard behavior of throwing an exception if the receipt has an error code.
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Submit Unsafe Transaction failed, status: {receipt.Status}", innerTransactionId.AsTxId(), (ResponseCode)receipt.Status);
            }
            if (includeRecord)
            {
                result.Record = await GetTransactionRecordAsync(context, innerTransactionId).ConfigureAwait(false);
            }
            return result!;

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }

            static TransactionID idFromTransactionBytes(ReadOnlyMemory<byte> transaction)
            {
                Transaction decodedTransaction;
                SignedTransaction decodedSignedTransaction;
                TransactionBody decodedTransactionBody;
                if (transaction.IsEmpty)
                {
                    throw new ArgumentOutOfRangeException(nameof(transaction), "Missing Transaction Bytes (was empty.)");
                }
                try
                {
                    decodedTransaction = Transaction.Parser.ParseFrom(transaction.ToArray());
                }
                catch (Exception pe)
                {
                    throw new ArgumentException(nameof(transaction), "The submitted bytes do not appear to belong to a transaction.", pe);
                }
                if (decodedTransaction.SignedTransactionBytes.IsEmpty)
                {
                    throw new ArgumentOutOfRangeException(nameof(transaction), "Missing Signed Transaction Bytes on Transaction (was empty.)");
                }
                try
                {
                    decodedSignedTransaction = Proto.SignedTransaction.Parser.ParseFrom(decodedTransaction.SignedTransactionBytes);
                }
                catch (Exception pe)
                {
                    throw new ArgumentException(nameof(transaction), "The submitted transaction does not appear to have a parsable signed transaction byte array.", pe);
                }
                try
                {
                    decodedTransactionBody = Proto.TransactionBody.Parser.ParseFrom(decodedSignedTransaction.BodyBytes);
                }
                catch (Exception pe)
                {
                    throw new ArgumentException(nameof(transaction), "The submitted bytes do not appear to have a transaction body.", pe);
                }
                if (decodedTransactionBody is null)
                {
                    throw new ArgumentException(nameof(transaction), "The submitted bytes do not appear to have a transaction body.");
                }
                if (decodedTransactionBody.TransactionID is null)
                {
                    throw new ArgumentException(nameof(transaction), "The submitted transaction bytes do not appear to contain a transaction id.");
                }
                return decodedTransactionBody.TransactionID;
            }
        }
    }
}
