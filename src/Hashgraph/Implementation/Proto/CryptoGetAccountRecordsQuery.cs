﻿using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class CryptoGetAccountRecordsQuery : INetworkQuery
    {
        Query INetworkQuery.CreateEnvelope()
        {
            return new Query { CryptoGetAccountRecords = this };
        }

        Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> INetworkQuery.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new CryptoService.CryptoServiceClient(channel).getAccountRecordsAsync;
        }

        void INetworkQuery.SetHeader(QueryHeader header)
        {
            Header = header;
        }

        internal CryptoGetAccountRecordsQuery(Address address) : this()
        {
            AccountID = new AccountID(address);
        }
    }
}
