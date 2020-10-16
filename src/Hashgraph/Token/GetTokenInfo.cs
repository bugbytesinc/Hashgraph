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
        /// Retrieves detailed information regarding a Token.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token to retrieve.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A detailed description of the contract instance.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        public async Task<TokenInfo> GetTokenInfoAsync(Address token, Action<IContext>? configure = null)
        {
            token = RequireInputParameter.Token(token);
            await using var context = CreateChildContext(configure);
            var query = new Query
            {
                TokenGetInfo = new TokenGetInfoQuery
                {
                    Header = Transactions.CreateAskCostHeader(),
                    Token = new TokenID(token)
                }
            };
            var response = await Transactions.ExecuteUnsignedAskRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
            long cost = (long)response.TokenGetInfo.Header.Cost;
            if (cost > 0)
            {
                var transactionId = Transactions.GetOrCreateTransactionID(context);
                query.TokenGetInfo.Header = await Transactions.CreateAndSignQueryHeaderAsync(context, cost, transactionId);
                response = await Transactions.ExecuteSignedRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
                ValidateResult.ResponseHeader(transactionId, getResponseHeader(response));
            }
            return response.TokenGetInfo.TokenInfo.ToTokenInfo();

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new TokenService.TokenServiceClient(channel);
                return async (Query query) => (await client.getTokenInfoAsync(query));
            }

            static ResponseHeader? getResponseHeader(Response response)
            {
                return response.TokenGetInfo?.Header;
            }
        }
    }
}
