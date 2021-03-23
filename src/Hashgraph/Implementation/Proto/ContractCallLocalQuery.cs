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

        internal ContractCallLocalQuery(QueryContractParams queryParameters) : this()
        {
            if (queryParameters is null)
            {
                throw new ArgumentNullException(nameof(queryParameters), "The query parameters are missing. Please check that the argument is not null.");
            }
            ContractID = new ContractID(queryParameters.Contract);
            Gas = queryParameters.Gas;
            FunctionParameters = ByteString.CopyFrom(Abi.EncodeFunctionWithArguments(queryParameters.FunctionName, queryParameters.FunctionArgs).Span);
            MaxResultSize = queryParameters.MaxAllowedReturnSize;
        }
    }
}
