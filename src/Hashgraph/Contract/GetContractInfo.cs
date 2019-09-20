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
        /// Retrieves detailed information regarding a Smart Contract Instance.
        /// </summary>
        /// <param name="contract">
        /// The Hedera Network Address of the Contract instance to retrieve.
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
        public async Task<ContractInfo> GetContractInfoAsync(Address contract, Action<IContext>? configure = null)
        {
            contract = RequireInputParameter.Contract(contract);
            var context = CreateChildContext(configure);
            var query = new Query
            {
                ContractGetInfo = new ContractGetInfoQuery
                {
                    Header = Transactions.CreateAskCostHeader(),
                    ContractID = Protobuf.ToContractID(contract)
                }
            };
            var response = await Transactions.ExecuteUnsignedAskRequestWithRetryAsync(context, query, getRequestMethod, getResponseCode);
            long cost = (long)response.ContractGetInfo.Header.Cost;
            if (cost > 0)
            {
                query.ContractGetInfo.Header = Transactions.CreateAndSignQueryHeader(context, cost, "Get Contract Info", out var transactionId);
                response = await Transactions.ExecuteSignedRequestWithRetryAsync(context, query, getRequestMethod, getResponseCode);
                ValidateResult.PreCheck(transactionId, getResponseCode(response));
            }
            return Protobuf.FromContractInfo(response.ContractGetInfo.ContractInfo);

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new SmartContractService.SmartContractServiceClient(channel);
                return async (Query query) => (await client.getContractInfoAsync(query));
            }

            static ResponseCodeEnum getResponseCode(Response response)
            {
                return response.ContractGetInfo?.Header?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
            }
        }
    }
}
