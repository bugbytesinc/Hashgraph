using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class TokenGetAccountNftInfosQuery : INetworkQuery
    {
        Query INetworkQuery.CreateEnvelope()
        {
            return new Query { TokenGetAccountNftInfos = this };
        }

        Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> INetworkQuery.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new TokenService.TokenServiceClient(channel).getAccountNftInfosAsync;
        }

        void INetworkQuery.SetHeader(QueryHeader header)
        {
            Header = header;
        }

        internal TokenGetAccountNftInfosQuery(Address account, long start, long count) : this()
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "The starting index must be greater than or equal to zero.");
            }
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "The number of tokens to retrieve must be greater than zero.");
            }
            AccountID = new AccountID(account);
            Start = start;
            End = start + count;
        }
    }
}
