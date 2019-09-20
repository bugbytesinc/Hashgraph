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
        /// Deletes an account from the network returning the remaining 
        /// crypto balance to the specified account.  Must be signed 
        /// by the account being deleted.
        /// </summary>
        /// <param name="accountToDelete">
        /// The account that will be deleted.
        /// </param>
        /// <param name="transferToAddress">
        /// The account that will receive any remaining balance from the deleted account.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> DeleteAccountAsync(Account accountToDelete, Address transferToAddress, Action<IContext>? configure = null)
        {
            accountToDelete = RequireInputParameter.AccountToDelete(accountToDelete);
            transferToAddress = RequireInputParameter.TransferToAddress(transferToAddress);
            var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId, "Delete Account");
            transactionBody.CryptoDelete = new CryptoDeleteTransactionBody
            {
                DeleteAccountID = Protobuf.ToAccountID(accountToDelete),
                TransferAccountID = Protobuf.ToAccountID(transferToAddress)
            };
            var request = Transactions.SignTransaction(transactionBody, accountToDelete, payer);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck.NodeTransactionPrecheckCode);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to delete account, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
            }
            var result = new TransactionReceipt();
            Protobuf.FillReceiptProperties(transactionId, receipt, result);
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Transaction transaction) => await client.cryptoDeleteAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
