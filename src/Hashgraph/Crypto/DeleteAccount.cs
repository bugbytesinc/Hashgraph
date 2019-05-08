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
        // Premature Implementation of this feature, does not
        // seem to be implemented on testnet at this moment.
        // Marked as private for now until we can confirm
        // it works and incorporate into automated test.
        private async Task<Address> DeleteAccountAsync(Address accountToDelete, Address transferAccount, Action<IContext>? configure = null)
        {
            Require.AccountToDeleteArgument(accountToDelete);
            Require.TransferAccountArgument(transferAccount);
            var context = CreateChildContext(configure);
            Require.GatewayInContext(context);
            Require.PayerInContext(context);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateEmptyTransactionBody(context, transactionId, "Delete Account");
            transactionBody.CryptoDelete = new CryptoDeleteTransactionBody
            {
                DeleteAccountID = Protobuf.ToAccountID(accountToDelete),
                TransferAccountID = Protobuf.ToAccountID(transferAccount)
            };
            var signatures = Transactions.SignProtoTransactionBody(transactionBody, context.Payer);
            var request = new Proto.Transaction
            {
                Body = transactionBody,
                Sigs = signatures
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, request, instantiateCryptoDeleteAsyncMethod, checkForRetry);
            Validate.ValidatePreCheckResult(response.NodeTransactionPrecheckCode);
            var receipt = await GetReceiptAsync(transactionId, context);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new PrecheckException($"Account was deleted, but unable to get receipt to confirm.  Code {receipt.Status}", PrecheckResponse.Ok);
            }
            return Protobuf.FromAccountID(receipt.AccountID);

            static Func<Proto.Transaction, Task<TransactionResponse>> instantiateCryptoDeleteAsyncMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Proto.Transaction transaction) => await client.cryptoDeleteAsync(transaction);
            }

            static bool checkForRetry(TransactionResponse response)
            {
                var code = response.NodeTransactionPrecheckCode;
                return
                    code == ResponseCodeEnum.Busy ||
                    code == ResponseCodeEnum.InvalidTransactionStart;
            }
        }
    }
}
