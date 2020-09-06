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
        /// Retrieves version information from the node.
        /// </summary>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// Version information regarding the gossip network node.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// 
        /// NOTE: Marked Internal because this is not yet implemented in testnet.
        internal async Task<VersionInfo> GetVersionInfoAsync(Action<IContext>? configure = null)
        {
            await using var context = CreateChildContext(configure);
            var query = new Query
            {
                NetworkGetVersionInfo = new NetworkGetVersionInfoQuery
                {
                    Header = Transactions.CreateAskCostHeader()
                }
            };
            var response = await Transactions.ExecuteUnsignedAskRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
            var cost = (long)response.NetworkGetVersionInfo.Header.Cost;
            if (cost > 0)
            {
                var transactionId = Transactions.GetOrCreateTransactionID(context);
                query.NetworkGetVersionInfo.Header = await Transactions.CreateAndSignQueryHeaderAsync(context, cost, transactionId);
                response = await Transactions.ExecuteSignedRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
                ValidateResult.ResponseHeader(transactionId, getResponseHeader(response));
            }
            return response.NetworkGetVersionInfo.ToVersionInfo();

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new NetworkService.NetworkServiceClient(channel);
                return async (Query query) => (await client.getVersionInfoAsync(query));
            }

            static ResponseHeader? getResponseHeader(Response response)
            {
                return response.NetworkGetVersionInfo?.Header;
            }
        }
    }
}
