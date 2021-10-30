using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class TokenUnpauseTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Unable to Continue Token, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { TokenUnpause = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { TokenUnpause = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new TokenService.TokenServiceClient(channel).pauseTokenAsync;
        }

        internal TokenUnpauseTransactionBody(Address token) : this()
        {
            Token = new TokenID(token);
        }
    }
}
