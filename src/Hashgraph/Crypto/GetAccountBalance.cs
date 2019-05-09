using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        public async Task<ulong> GetAccountBalanceAsync(Address address, Action<IContext>? configure = null)
        {
            Require.AddressArgument(address);
            var context = CreateChildContext(configure);
            var gateway = Require.GatewayInContext(context);
            var payer = Require.PayerInContext(context);
            var transfers = Transactions.CreateCryptoTransferList((payer, -context.FeeLimit), (gateway, context.FeeLimit));
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateCryptoTransferTransactionBody(context, transfers, transactionId, "Get Account Balance");
            var signatures = Transactions.SignProtoTransactionBody(transactionBody, payer);
            var query = new Query
            {
                CryptogetAccountBalance = new CryptoGetAccountBalanceQuery
                {
                    Header = Transactions.CreateProtoQueryHeader(transactionBody, signatures),
                    AccountID = Protobuf.ToAccountID(address)
                }
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, query, instantiateExecuteCryptoGetBalanceAsyncMethod, checkForRetry);
            Validate.ValidatePreCheckResult(transactionId, response.Header.NodeTransactionPrecheckCode);
            return response.Balance;

            static Func<Query, Task<CryptoGetAccountBalanceResponse>> instantiateExecuteCryptoGetBalanceAsyncMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Query query) => (await client.cryptoGetBalanceAsync(query)).CryptogetAccountBalance;
            }

            static bool checkForRetry(CryptoGetAccountBalanceResponse response)
            {
                var code = response.Header.NodeTransactionPrecheckCode;
                return
                    code == ResponseCodeEnum.Busy ||
                    code == ResponseCodeEnum.InvalidTransactionStart;
            }
        }
    }
}
