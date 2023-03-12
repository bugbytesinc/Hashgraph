using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Proto;

public sealed partial class TokenFeeScheduleUpdateTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { TokenFeeScheduleUpdate = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { TokenFeeScheduleUpdate = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(
        GrpcChannel channel)
    {
        return new TokenService.TokenServiceClient(channel).updateTokenFeeScheduleAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Unable to Update Royalties, status: {0}", result.Receipt.Status), result);
        }
    }

    internal TokenFeeScheduleUpdateTransactionBody(Address token, IEnumerable<IRoyalty>? royalties) : this()
    {
        TokenId = new TokenID(token);
        // Note: Null & Empty are Valid, they will clear the list of fees.
        if (royalties is not null)
        {
            CustomFees.AddRange(royalties.Select(royalty => new CustomFee(royalty)));
        }
    }
}