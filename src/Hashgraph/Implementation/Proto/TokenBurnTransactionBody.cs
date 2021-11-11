using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Proto
{
    public sealed partial class TokenBurnTransactionBody : INetworkTransaction
    {
        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { TokenBurn = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { TokenBurn = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new TokenService.TokenServiceClient(channel).burnTokenAsync;
        }

        void INetworkTransaction.CheckReceipt(NetworkResult result)
        {
            if (result.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException(string.Format("Unable to Burn Token Coins, status: {0}", result.Receipt.Status), result);
            }
        }

        internal TokenBurnTransactionBody(Hashgraph.Address token, ulong amount) : this()
        {
            if (amount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "The token amount must be greater than zero.");
            }
            Token = new TokenID(token);
            Amount = amount;
        }
        internal TokenBurnTransactionBody(Hashgraph.Address asset, IEnumerable<long> serialNumbers) : this()
        {
            if (serialNumbers is null)
            {
                throw new ArgumentOutOfRangeException(nameof(serialNumbers), "The list of serial numbers must not be null.");
            }
            Token = new TokenID(asset);
            SerialNumbers.AddRange(serialNumbers);
            if (SerialNumbers.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(serialNumbers), "The list of serial numbers must not be empty.");
            }
        }
        internal TokenBurnTransactionBody(Hashgraph.Asset asset) : this()
        {
            if (asset is null || asset == Hashgraph.Asset.None)
            {
                throw new ArgumentOutOfRangeException(nameof(asset), "The asset cannot be null or empty.");
            }
            Token = new TokenID(asset);
            SerialNumbers.Add(asset.SerialNum);
        }
    }
}
