using Grpc.Core;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class TransactionGetReceiptQuery : INetworkQuery
    {
        Query INetworkQuery.CreateEnvelope()
        {
            return new Query { TransactionGetReceipt = this };
        }

        Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> INetworkQuery.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new CryptoService.CryptoServiceClient(channel).getTransactionReceiptsAsync;
        }

        void INetworkQuery.SetHeader(QueryHeader header)
        {
            Header = header;
        }

        internal TransactionGetReceiptQuery(TransactionID transactionId, bool includeDuplicates, bool includeChildren) : this()
        {
            TransactionID = transactionId;
            IncludeDuplicates = includeDuplicates;
            IncludeChildReceipts = includeChildren;
        }
    }
}
