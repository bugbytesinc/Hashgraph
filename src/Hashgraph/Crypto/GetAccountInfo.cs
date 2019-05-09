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
            var gateway = Require.GatewayInContext(context);
            var payer = Require.PayerInContext(context);
            var transfers = Transactions.CreateCryptoTransferList((payer, -context.FeeLimit), (gateway, context.FeeLimit));
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateCryptoTransferTransactionBody(context, transfers, transactionId, "Get Account Info");
            var signatures = Transactions.SignProtoTransactionBody(transactionBody, payer);
            var query = new Query
            {
                CryptoGetInfo = new CryptoGetInfoQuery
                {
                    Header = Transactions.CreateProtoQueryHeader(transactionBody, signatures),
                    AccountID = Protobuf.ToAccountID(address)
                }
            };
            var data = await Transactions.ExecuteRequestWithRetryAsync(context, query, instantiateExecuteCryptoGetInfoAsyncMethod, checkForRetry);
            Validate.ValidatePreCheckResult(transactionId, data.Header.NodeTransactionPrecheckCode);
            return Protobuf.FromAccountInfo(data.AccountInfo);

            static Func<Query, Task<CryptoGetInfoResponse>> instantiateExecuteCryptoGetInfoAsyncMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Query query) => (await client.getAccountInfoAsync(query)).CryptoGetInfo;
            }

            static bool checkForRetry(CryptoGetInfoResponse response)
            {
                var code = response.Header.NodeTransactionPrecheckCode;
                return
                    code == ResponseCodeEnum.Busy ||
                    code == ResponseCodeEnum.InvalidTransactionStart;
            }
        }
    }
}
