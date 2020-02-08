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
        /// Calls a smart contract function locally on the gateway node.
        /// </summary>
        /// <remarks>
        /// This is performed locally on the gateway node. It cannot change the state of the contract instance 
        /// (and so, cannot spend anything from the instance's cryptocurrency account). It will not have a 
        /// consensus timestamp nor a record or a receipt. The response will contain the output returned 
        /// by the function call.  
        /// </remarks>
        /// <param name="queryParameters">
        /// The parameters identifying the contract and function method to call.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// The results from the local contract query call.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        public async Task<ContractCallResult> QueryContractAsync(QueryContractParams queryParameters, Action<IContext>? configure = null)
        {
            queryParameters = RequireInputParameter.QueryParameters(queryParameters);
            await using var context = CreateChildContext(configure);
            var query = new Query
            {
                ContractCallLocal = new ContractCallLocalQuery
                {
                    Header = Transactions.CreateAskCostHeader(),
                    ContractID = Protobuf.ToContractID(queryParameters.Contract),
                    Gas = queryParameters.Gas,
                    FunctionParameters = Abi.EncodeFunctionWithArguments(queryParameters.FunctionName, queryParameters.FunctionArgs).ToByteString(),
                    MaxResultSize = queryParameters.MaxAllowedReturnSize
                }
            };
            var response = await Transactions.ExecuteUnsignedAskRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
            long cost = (long)response.ContractCallLocal.Header.Cost;
            if (cost > 0)
            {
                var transactionId = Transactions.GetOrCreateTransactionID(context);
                query.ContractCallLocal.Header = await Transactions.CreateAndSignQueryHeaderAsync(context, cost + queryParameters.ReturnValueCharge, "Query Contract Local Call", transactionId);
                response = await Transactions.ExecuteSignedRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
                ValidateResult.ResponseHeader(transactionId, getResponseHeader(response));
            }
            return Protobuf.FromContractCallResult(response.ContractCallLocal.FunctionResult);

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new SmartContractService.SmartContractServiceClient(channel);
                return async (Query query) => (await client.contractCallLocalMethodAsync(query));
            }

            static ResponseHeader? getResponseHeader(Response response)
            {
                return response.ContractCallLocal?.Header;
            }
        }
    }
}
