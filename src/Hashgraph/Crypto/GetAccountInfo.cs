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
        /// Retrieves detailed information regarding a Hedera Network Account.
        /// </summary>
        /// <param name="address">
        /// The Hedera Network Address to retrieve detailed information of.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A detailed description of the account.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        public async Task<AccountInfo> GetAccountInfoAsync(Address address, Action<IContext>? configure = null)
        {
            address = RequireInputParameter.Address(address);
            await using var context = CreateChildContext(configure);
            var query = new Query
            {
                CryptoGetInfo = new CryptoGetInfoQuery
                {
                    Header = Transactions.CreateAskCostHeader(),
                    AccountID = Protobuf.ToAccountID(address)
                }
            };
            var response = await Transactions.ExecuteUnsignedAskRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
            long cost = (long)response.CryptoGetInfo.Header.Cost;
            if (cost > 0)
            {
                var transactionId = Transactions.GetOrCreateTransactionID(context);
                query.CryptoGetInfo.Header = await Transactions.CreateAndSignQueryHeaderAsync(context, cost, "Get Account Info", transactionId);
                response = await Transactions.ExecuteSignedRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
                ValidateResult.ResponseHeader(transactionId, getResponseHeader(response));
            }
            return Protobuf.FromAccountInfo(response.CryptoGetInfo.AccountInfo);

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Query query) => (await client.getAccountInfoAsync(query));
            }

            static ResponseHeader? getResponseHeader(Response response)
            {
                return response.CryptoGetInfo?.Header;
            }
        }
    }
}
