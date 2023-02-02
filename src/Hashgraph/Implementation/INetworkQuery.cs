using Grpc.Core;
using Grpc.Net.Client;
using Proto;
using System;
using System.Threading;

namespace Hashgraph.Implementation;

internal interface INetworkQuery
{
    Query CreateEnvelope();
    Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> InstantiateNetworkRequestMethod(GrpcChannel channel);
    void SetHeader(QueryHeader header);
    void CheckResponse(TransactionID transactionId, Response response)
    {
        var header = response.ResponseHeader;
        if (header == null)
        {
            throw new PrecheckException($"Query Failed to Produce a Response.", transactionId.AsTxId(), ResponseCode.Unknown, 0);
        }
        if (header.NodeTransactionPrecheckCode == ResponseCodeEnum.InsufficientTxFee)
        {
            throw new PrecheckException($"Query Failed because the network changed the published price of the Query before the paying transaction could be signed and submitted: {header.NodeTransactionPrecheckCode}", transactionId.AsTxId(), (ResponseCode)header.NodeTransactionPrecheckCode, header.Cost);
        }
        if (header.NodeTransactionPrecheckCode != ResponseCodeEnum.Ok)
        {
            throw new PrecheckException($"Query Transaction Failed Pre-Check: {header.NodeTransactionPrecheckCode}", transactionId.AsTxId(), (ResponseCode)header.NodeTransactionPrecheckCode, header.Cost);
        }
    }
}