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
        /// Retrieves the balance in tinybars from the network for a given contract.
        /// </summary>
        /// <param name="contract">
        /// The hedera network contract address to retrieve the balance of.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// The balance of the associated contract.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        public async Task<ulong> GetContractBalanceAsync(Address contract, Action<IContext>? configure = null)
        {
            contract = RequireInputParameter.Contract(contract);
            await using var context = CreateChildContext(configure);
            var query = new Query
            {
                CryptogetAccountBalance = new CryptoGetAccountBalanceQuery
                {
                    Header = Transactions.CreateAskCostHeader(),
                    ContractID = Protobuf.ToContractID(contract)
                }
            };
            var response = await Transactions.ExecuteUnsignedAskRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
            var cost = (long)response.CryptogetAccountBalance.Header.Cost;
            if (cost > 0)
            {
                var transactionId = Transactions.GetOrCreateTransactionID(context);
                query.CryptogetAccountBalance.Header = await Transactions.CreateAndSignQueryHeaderAsync(context, cost, transactionId);
                response = await Transactions.ExecuteSignedRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
                ValidateResult.ResponseHeader(transactionId, getResponseHeader(response));
            }
            return response.CryptogetAccountBalance.Balance;

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Query query) => (await client.cryptoGetBalanceAsync(query));
            }

            static ResponseHeader? getResponseHeader(Response response)
            {
                return response.CryptogetAccountBalance?.Header;
            }
        }
    }
}
