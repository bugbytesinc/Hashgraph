using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class TokenPauseTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Unable to Pause Token, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { TokenPause = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { TokenPause = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new TokenService.TokenServiceClient(channel).pauseTokenAsync;
        }

        internal TokenPauseTransactionBody(Address token) : this()
        {
            Token = new TokenID(token);
        }
    }
}
