using Grpc.Core;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class TokenWipeAccountTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Unable to Confiscate Token, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { TokenWipe = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { TokenWipe = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new TokenService.TokenServiceClient(channel).wipeTokenAccountAsync;
        }

        internal TokenWipeAccountTransactionBody(Hashgraph.Address token, Hashgraph.Address address, ulong amount) : this()
        {
            if (amount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "The amount to confiscate must be greater than zero.");
            }
            Token = new TokenID(token);
            Account = new AccountID(address);
            Amount = amount;
        }
    }
}
