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
            Require.GatewayInContext(context);
            Require.PayerInContext(context);
            var transfers = Transactions.CreateProtoTransferList((context.Payer, -context.Fee), (context.Gateway, context.Fee));
            var transactionId = Transactions.GetOrCreateProtoTransactionID(context);
            var transactionBody = Transactions.CreateProtoTransactionBody(context, transfers, transactionId, "Get Account Balance");
            var signatures = Transactions.SignProtoTransactionBody(transactionBody, context.Payer);
            var query = new Query
            {
                CryptogetAccountBalance = new CryptoGetAccountBalanceQuery
                {
                    Header = Transactions.CreateProtoQueryHeader(transactionBody, signatures),
                    AccountID = Protobuf.ToAccountID(address)
                }
            };
            var data = await Transactions.ExecuteQueryWithRetryAsync(context, query, instantiateExecuteCryptoGetBalanceAsyncMethod, extractCryptoAccountBalanceResponseHeader);
            Validate.validatePreCheckResult(data.Header);
            return data.Balance;

            Func<Query, Task<CryptoGetAccountBalanceResponse>> instantiateExecuteCryptoGetBalanceAsyncMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return executeCryptoGetBalanceAsync;

                async Task<CryptoGetAccountBalanceResponse> executeCryptoGetBalanceAsync(Query query)
                {
                    return (await client.cryptoGetBalanceAsync(query)).CryptogetAccountBalance;
                }
            }

            static ResponseHeader extractCryptoAccountBalanceResponseHeader(CryptoGetAccountBalanceResponse response)
            {
                return response.Header;
            }
        }
    }
}
