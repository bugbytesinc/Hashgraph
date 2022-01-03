using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Proto
{
    public sealed partial class TokenWipeAccountTransactionBody : INetworkTransaction
    {
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

        void INetworkTransaction.CheckReceipt(NetworkResult result)
        {
            if (result.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException(string.Format("Unable to Confiscate Token, status: {0}", result.Receipt.Status), result);
            }
        }

        internal TokenWipeAccountTransactionBody(Hashgraph.Address token, AddressOrAlias address, ulong amount) : this()
        {
            if (amount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "The amount to confiscate must be greater than zero.");
            }
            Token = new TokenID(token);
            Account = new AccountID(address);
            Amount = amount;
        }

        internal TokenWipeAccountTransactionBody(Asset asset, AddressOrAlias address) : this()
        {
            if (Hashgraph.Asset.None.Equals(asset))
            {
                throw new ArgumentOutOfRangeException(nameof(asset), "The asset to confiscate is missing.");
            }
            if (Hashgraph.Address.None.Equals(address))
            {
                throw new ArgumentOutOfRangeException(nameof(address), "The account Addresss can not be empty or None.  Please provide a valid value.");
            }
            Token = new TokenID(asset);
            Account = new AccountID(address);
            SerialNumbers.Add(asset.SerialNum);
        }
        internal TokenWipeAccountTransactionBody(Address token, IEnumerable<long> serialNumbers, AddressOrAlias address) : this()
        {
            if (Hashgraph.Asset.None.Equals(token))
            {
                throw new ArgumentOutOfRangeException(nameof(token), "The asset token type to confiscate is missing.");
            }
            if (Hashgraph.Address.None.Equals(address))
            {
                throw new ArgumentOutOfRangeException(nameof(address), "The account Addresss can not be empty or None.  Please provide a valid value.");
            }
            Token = new TokenID(token);
            Account = new AccountID(address);
            SerialNumbers.AddRange(serialNumbers);
        }
    }
}
