using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class CryptoGetInfoQuery : INetworkQuery
    {
        Query INetworkQuery.CreateEnvelope()
        {
            return new Query { CryptoGetInfo = this };
        }

        Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> INetworkQuery.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new CryptoService.CryptoServiceClient(channel).getAccountInfoAsync;
        }

        void INetworkQuery.SetHeader(QueryHeader header)
        {
            Header = header;
        }

        internal CryptoGetInfoQuery(AddressOrAlias address) : this()
        {
            AccountID = new AccountID(address);
        }
    }
}
