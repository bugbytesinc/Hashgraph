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
        /// Transfer tinybars from one account to another.
        /// </summary>
        /// <param name="fromAccount">
        /// The account to transfer the tinybars from.  It will sign the 
        /// transaction, but may not necessarily be the account 
        /// <see cref="IContext.Payer">paying</see> the transaction fee.
        /// </param>
        /// <param name="toAddress">
        /// The address receiving the tinybars.
        /// </param>
        /// <param name="amount">
        /// The amount of tinybars to transfer.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer receipt indicating success of the operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> TransferAsync(Account fromAccount, Address toAddress, long amount, Action<IContext>? configure = null)
        {
            return TransferImplementationAsync<TransactionReceipt>(fromAccount, toAddress, amount, configure);
        }
        /// <summary>
        /// Transfer tinybars from one account to another.
        /// </summary>
        /// <param name="fromAccount">
        /// The account to transfer the tinybars from.  It will sign the 
        /// transaction, but may not necessarily be the account 
        /// <see cref="IContext.Payer">paying</see> the transaction fee.
        /// </param>
        /// <param name="toAddress">
        /// The address receiving the tinybars.
        /// </param>
        /// <param name="amount">
        /// The amount of tinybars to transfer.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer record describing the details of the concensus transaction.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransferRecord> TransferWithRecordAsync(Account fromAccount, Address toAddress, long amount, Action<IContext>? configure = null)
        {
            return TransferImplementationAsync<TransferRecord>(fromAccount, toAddress, amount, configure);
        }
        /// <summary>
        /// Internal implementation for Transfer Crypto.
        /// Returns either a receipt or record or throws
        /// an exception.
        /// </summary>
        private async Task<TResult> TransferImplementationAsync<TResult>(Account fromAccount, Address toAddress, long amount, Action<IContext>? configure = null) where TResult : new()
        {
            fromAccount = RequireInputParameter.FromAccount(fromAccount);
            toAddress = RequireInputParameter.ToAddress(toAddress);
            amount = RequireInputParameter.Amount(amount);
            var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transfers = Transactions.CreateCryptoTransferList((fromAccount, -amount), (toAddress, amount));
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateCryptoTransferTransactionBody(context, transfers, transactionId, "Transfer Crypto");
            var request = Transactions.SignTransaction(transactionBody, fromAccount, payer);
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, response.NodeTransactionPrecheckCode);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to execute crypto transfer, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is TransferRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                Protobuf.FillRecordProperties(transactionId, receipt, record, rec);
                rec.Transfers = Protobuf.FromTransferList(record.TransferList);
            }
            else if (result is TransactionReceipt rcpt)
            {
                Protobuf.FillReceiptProperties(transactionId, receipt, rcpt);
            }
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Transaction request) => await client.cryptoTransferAsync(request);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
