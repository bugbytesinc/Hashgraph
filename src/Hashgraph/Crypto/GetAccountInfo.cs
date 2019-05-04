using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        public async Task<AccountInfo> GetAccountInfoAsync(Address address, Action<IContext>? configure = null)
        {
            Require.AddressArgument(address);
            var context = CreateChildContext(configure);
            Require.GatewayInContext(context);
            Require.PayerInContext(context);
            var transfers = Transactions.CreateProtoTransferList((context.Payer, -context.Fee), (context.Gateway, context.Fee));
            var transactionId = Transactions.GetOrCreateProtoTransactionID(context);
            var transactionBody = Transactions.CreateProtoTransactionBody(context, transfers, transactionId, "Get Account Info");
            var signatures = Transactions.SignProtoTransactionBody(transactionBody, context.Payer);
            var query = new Query
            {
                CryptoGetInfo = new CryptoGetInfoQuery
                {
                    Header = Transactions.CreateProtoQueryHeader(transactionBody, signatures),
                    AccountID = Protobuf.ToAccountID(address)
                }
            };
            var data = await Transactions.ExecuteQueryWithRetryAsync(context, query, instantiateExecuteCryptoGetInfoAsyncMethod, extractCryptoGetInfoResponseHeader);
            Validate.validatePreCheckResult(data.Header);
            return Protobuf.FromAccountInfo(data.AccountInfo);

            Func<Query, Task<CryptoGetInfoResponse>> instantiateExecuteCryptoGetInfoAsyncMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return executeCryptoGetInfoAsync;

                async Task<CryptoGetInfoResponse> executeCryptoGetInfoAsync(Query query)
                {
                    return (await client.getAccountInfoAsync(query)).CryptoGetInfo;
                }
            }

            static ResponseHeader extractCryptoGetInfoResponseHeader(CryptoGetInfoResponse response)
            {
                return response.Header;
            }
        }
    }
}
