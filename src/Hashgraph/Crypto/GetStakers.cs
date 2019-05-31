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
        // NOTE: Marked internal at this point because it is not yet implemented by the network
        internal async Task<Dictionary<Address,long>> GetStakers(Address address, Action<IContext>? configure = null)
        {
            address = RequireInputParameter.Address(address);
            var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transfers = Transactions.CreateCryptoTransferList((payer, -context.FeeLimit), (gateway, context.FeeLimit));
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateCryptoTransferTransactionBody(context, transfers, transactionId, "Get Account Info");
            var query = new Query
            {
                CryptoGetProxyStakers = new CryptoGetStakersQuery
                {
                    Header = Transactions.SignQueryHeader(transactionBody, payer),
                    AccountID = Protobuf.ToAccountID(address)
                }
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, query, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, getResponseCode(response));
            return response.CryptoGetProxyStakers.Stakers.ProxyStaker.ToDictionary(ps => Protobuf.FromAccountID(ps.AccountID), ps => ps.Amount);

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Query query) => (await client.getStakersByAccountIDAsync(query));
            }

            static ResponseCodeEnum getResponseCode(Response response)
            {
                return response.CryptoGetProxyStakers?.Header?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
            }
        }
    }
}
