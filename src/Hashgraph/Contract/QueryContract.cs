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
        /// <exception cref="ContractException">If the request was accepted by the network but the cotnract failed for
        /// some reason.  Contains additional information returned from the contract virual machine.  Only thrown if
        /// the <see cref="QueryContractParams.ThrowOnFail"/> is set to <code>true</code>, the default, otherwise
        /// the method returns a <see cref="ContractCallResult"/> with the same information.</exception>
        public async Task<ContractCallResult> QueryContractAsync(QueryContractParams queryParameters, Action<IContext>? configure = null)
        {
            queryParameters = RequireInputParameter.QueryParameters(queryParameters);
            await using var context = CreateChildContext(configure);
            var query = new Query
            {
                ContractCallLocal = new ContractCallLocalQuery
                {
                    Header = Transactions.CreateAskCostHeader(),
                    ContractID = new ContractID(queryParameters.Contract),
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
                query.QueryHeader = await Transactions.CreateAndSignQueryHeaderAsync(context, cost + queryParameters.ReturnValueCharge, transactionId);
                response = await Transactions.ExecuteSignedQueryWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
                var header = getResponseHeader(response);
                if (header == null)
                {
                    throw new PrecheckException($"Transaction Failed to Produce a Response.", transactionId.ToTxId(), ResponseCode.Unknown, 0);
                }
                if(response.ContractCallLocal?.FunctionResult == null)
                {
                    throw new PrecheckException($"Transaction Failed Pre-Check: {header.NodeTransactionPrecheckCode}", transactionId.ToTxId(), (ResponseCode)header.NodeTransactionPrecheckCode, header.Cost);
                }
                if (queryParameters.ThrowOnFail && header.NodeTransactionPrecheckCode != ResponseCodeEnum.Ok)
                {
                    throw new ContractException(
                        $"Contract Query Failed with Code: {header.NodeTransactionPrecheckCode}",
                        transactionId.ToTxId(),
                        (ResponseCode)header.NodeTransactionPrecheckCode,
                        header.Cost,
                        response.ContractCallLocal.FunctionResult.ToContractCallResult());
                }
            }
            return response.ContractCallLocal.FunctionResult.ToContractCallResult();

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
