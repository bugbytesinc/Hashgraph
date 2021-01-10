#pragma warning disable IDE0066
using Hashgraph.Implementation;
using Proto;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Retrieves the receipt from the network matching the transaction
        /// id.  Will wait for the disposition of the receipt to be known.
        /// </summary>
        /// <param name="transaction">
        /// Transaction identifier of the receipt.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// The receipt matching the transaction id, if found and marked
        /// sucessfull, otherwise a <see cref="TransactionException"/> is 
        /// not found or returns an error status.
        /// </returns>
        /// <exception cref="TransactionException">If the network has no record of the transaction or request has invalid or had missing data.</exception>
        public async Task<TransactionReceipt> GetReceiptAsync(TxId transaction, Action<IContext>? configure = null)
        {
            transaction = RequireInputParameter.Transaction(transaction);
            await using var context = CreateChildContext(configure);
            var transactionId = new TransactionID(transaction);
            var receipt = await Transactions.GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to retreive receipt, status: {receipt.Status}", transaction, (ResponseCode)receipt.Status);
            }
            return receipt.ToTransactionReceipt(transactionId);
        }
        /// <summary>
        /// Retreives all known receipts from the network having the given
        /// transaction ID.  The method will wait for the disposition of at
        /// least one receipt to be known.  Receipts with failure codes do
        /// not cause an exception to be raised.
        /// </summary>
        /// <param name="transaction">
        /// Transaction identifier of the receipt.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A list of receipts having the identified transaction.  The list
        /// may be empty or contain multiple entries.
        /// </returns>
        /// <exception cref="ConsensusException">if the receipt is known to exist but has not reached consensus 
        /// within the alloted time to wait for a return from this method or the network has been too busy to 
        /// respond.</exception>
        public async Task<ReadOnlyCollection<TransactionReceipt>> GetAllReceiptsAsync(TxId transaction, Action<IContext>? configure = null)
        {
            transaction = RequireInputParameter.Transaction(transaction);
            await using var context = CreateChildContext(configure);
            var transactionId = new TransactionID(transaction);
            var query = new Query
            {
                TransactionGetReceipt = new TransactionGetReceiptQuery
                {
                    TransactionID = transactionId,
                    IncludeDuplicates = true
                }
            };
            var response = await Transactions.ExecuteNetworkRequestWithRetryAsync(context, query, query.InstantiateNetworkRequestMethod, shouldRetry);
            var responseCode = response.TransactionGetReceipt.Header.NodeTransactionPrecheckCode;
            if (responseCode == ResponseCodeEnum.Busy)
            {
                throw new ConsensusException("Network failed to respond to request for a transaction receipt, it is too busy. It is possible the network may still reach concensus for this transaction.", transactionId.ToTxId(), (ResponseCode)responseCode);
            }
            return response.TransactionGetReceipt.DuplicateTransactionReceipts.ToTransactionReceiptList(response.TransactionGetReceipt.Receipt, transactionId);

            static bool shouldRetry(Response response)
            {
                return
                    response.TransactionGetReceipt?.Header?.NodeTransactionPrecheckCode == ResponseCodeEnum.Busy ||
                    response.TransactionGetReceipt?.Receipt?.Status == ResponseCodeEnum.Unknown;
            }
        }
    }
}
