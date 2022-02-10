using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class TokenUnfreezeAccountTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { TokenUnfreeze = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { TokenUnfreeze = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
    {
        return new TokenService.TokenServiceClient(channel).unfreezeTokenAccountAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Unable to Resume Token, status: {0}", result.Receipt.Status), result);
        }
    }

    internal TokenUnfreezeAccountTransactionBody(Address token, Address address) : this()
    {
        Token = new TokenID(token);
        Account = new AccountID(address);
    }
}