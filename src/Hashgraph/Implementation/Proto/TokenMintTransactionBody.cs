using Grpc.Core;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class TokenMintTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Unable to Mint Token Coins, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { TokenMint = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { TokenMint = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new TokenService.TokenServiceClient(channel).mintTokenAsync;
        }

        internal TokenMintTransactionBody(Hashgraph.Address token, ulong amount) : this()
        {
            if (amount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "The token amount must be greater than zero.");
            }
            Token = new TokenID(token);
            Amount = amount;
        }
    }
}
