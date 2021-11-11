using Google.Protobuf;
using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class ContractCallLocalQuery : INetworkQuery
    {
        private bool _throwOnFail = true;
        
        Query INetworkQuery.CreateEnvelope()
        {
            return new Query { ContractCallLocal = this };
        }

        Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> INetworkQuery.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new SmartContractService.SmartContractServiceClient(channel).contractCallLocalMethodAsync;
        }

        void INetworkQuery.SetHeader(QueryHeader header)
        {
            Header = header;
        }

        void INetworkQuery.CheckResponse(TransactionID transactionId, Response response)
        {
            var header = response.ResponseHeader;
            if (header == null)
            {
                throw new PrecheckException($"Transaction Failed to Produce a Response.", transactionId.AsTxId(), ResponseCode.Unknown, 0);
            }
            else if (response.ContractCallLocal?.FunctionResult == null)
            {
                throw new PrecheckException($"Transaction Failed Pre-Check: {header.NodeTransactionPrecheckCode}", transactionId.AsTxId(), (ResponseCode)header.NodeTransactionPrecheckCode, header.Cost);
            }
            else if (_throwOnFail && header.NodeTransactionPrecheckCode != ResponseCodeEnum.Ok)
            {
                throw new ContractException(
                    $"Contract Query Failed with Code: {header.NodeTransactionPrecheckCode}",
                    transactionId.AsTxId(),
                    (ResponseCode)header.NodeTransactionPrecheckCode,
                    header.Cost,
                    new ContractCallResult(response));
            }
        }

        internal ContractCallLocalQuery(QueryContractParams queryParameters) : this()
        {
            if (queryParameters is null)
            {
                throw new ArgumentNullException(nameof(queryParameters), "The query parameters are missing. Please check that the argument is not null.");
            }
            ContractID = new ContractID(queryParameters.Contract);
            Gas = queryParameters.Gas;
            FunctionParameters = ByteString.CopyFrom(Abi.EncodeFunctionWithArguments(queryParameters.FunctionName, queryParameters.FunctionArgs).Span);
            _throwOnFail = queryParameters.ThrowOnFail;
        }
    }
}
