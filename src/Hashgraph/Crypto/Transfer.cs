using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        public async Task<string> TransferAsync(Account fromAccount, Address toAddress, long amount, Action<IContext>? configure = null)
        {
            Require.FromAccountArgument(fromAccount);
            Require.ToAddressArgument(toAddress);
            Require.AmountArgument(amount);
            var context = CreateChildContext(configure);
            Require.GatewayInContext(context);
            Require.PayerInContext(context);
            var transfers = Transactions.CreateCryptoTransferList((fromAccount, -amount), (toAddress, amount));
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateCryptoTransferTransactionBody(context, transfers, transactionId, "Transfer Crypto");
            var signatures = Transactions.SignProtoTransactionBody(transactionBody, context.Payer, fromAccount);
            var request = new Proto.Transaction
            {
                Body = transactionBody,
                Sigs = signatures
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, request, instantiateExecuteCryptoGetBalanceAsyncMethod, checkForRetry);
            Validate.ValidatePreCheckResult(transactionId, response.NodeTransactionPrecheckCode);
            var record = await GetFastRecordAsync(transactionId, context);
            if (record.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Transfer failed. Status: {record.Receipt.Status}");
            }
            return record.ToString();

            static Func<Proto.Transaction, Task<TransactionResponse>> instantiateExecuteCryptoGetBalanceAsyncMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Proto.Transaction request) => await client.cryptoTransferAsync(request);
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
