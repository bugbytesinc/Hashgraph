using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class CryptoGetAccountBalanceQuery : INetworkQuery
    {
        Query INetworkQuery.CreateEnvelope()
        {
            return new Query { CryptogetAccountBalance = this };
        }

        Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> INetworkQuery.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new CryptoService.CryptoServiceClient(channel).cryptoGetBalanceAsync;
        }

        void INetworkQuery.SetHeader(QueryHeader header)
        {
            Header = header;
        }

        internal static CryptoGetAccountBalanceQuery ForContract(Address contract)
        {
            return new CryptoGetAccountBalanceQuery
            {
                ContractID = new ContractID(contract)
            };
        }
        internal static CryptoGetAccountBalanceQuery ForAccount(Address account)
        {
            return new CryptoGetAccountBalanceQuery
            {
                AccountID = new AccountID(account)
            };
        }
    }
}
