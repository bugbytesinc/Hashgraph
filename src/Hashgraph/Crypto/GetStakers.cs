using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Retreives the accounts that are proxy staking to this account.
        /// </summary>
        /// <param name="address">
        /// The Hedera Network Address to retrieve the stakers of.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A dictionary mapping account addresses to the amount of stake.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        ///
        /// <remarks>
        /// Marked internal at this point because it is not yet implemented by the network
        /// </remarks>
        internal async Task<Dictionary<Address, long>> GetStakers(Address address, Action<IContext>? configure = null)
        {
            address = RequireInputParameter.Address(address);
            await using var context = CreateChildContext(configure);
            var query = new Query
            {
                CryptoGetProxyStakers = new CryptoGetStakersQuery
                {
                    Header = Transactions.CreateAskCostHeader(),
                    AccountID = new AccountID(address)
                }
            };
            var response = await Transactions.ExecuteUnsignedAskRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
            long cost = (long)response.CryptoGetProxyStakers.Header.Cost;
            if (cost > 0)
            {
                var transactionId = Transactions.GetOrCreateTransactionID(context);
                query.QueryHeader = await Transactions.CreateAndSignQueryHeaderAsync(context, cost, transactionId);
                response = await Transactions.ExecuteSignedQueryWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
                ValidateResult.ResponseHeader(transactionId, getResponseHeader(response));
            }
            return response.CryptoGetProxyStakers.Stakers.ProxyStaker.ToDictionary(ps => ps.AccountID.ToAddress(), ps => ps.Amount);

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Query query) => (await client.getStakersByAccountIDAsync(query));
            }

            static ResponseHeader? getResponseHeader(Response response)
            {
                return response.CryptoGetProxyStakers?.Header;
            }
        }
    }
}
