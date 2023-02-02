using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class ContractGetBytecodeQuery : INetworkQuery
{
    Query INetworkQuery.CreateEnvelope()
    {
        return new Query { ContractGetBytecode = this };
    }

    Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> INetworkQuery.InstantiateNetworkRequestMethod(
        GrpcChannel channel)
    {
        return new SmartContractService.SmartContractServiceClient(channel).ContractGetBytecodeAsync;
    }

    void INetworkQuery.SetHeader(QueryHeader header)
    {
        Header = header;
    }

    internal ContractGetBytecodeQuery(Address contract) : this()
    {
        ContractID = new ContractID(contract);
    }
}