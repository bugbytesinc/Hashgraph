using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class TokenFreezeAccountTransactionBody : INetworkTransaction
    {
        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { TokenFreeze = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { TokenFreeze = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new TokenService.TokenServiceClient(channel).freezeTokenAccountAsync;
        }

        void INetworkTransaction.CheckReceipt(NetworkResult result)
        {
            if (result.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException(string.Format("Unable to Suspend Token, status: {0}", result.Receipt.Status), result);
            }
        }

        internal TokenFreezeAccountTransactionBody(Address token, AddressOrAlias address) : this()
        {
            Token = new TokenID(token);
            Account = new AccountID(address);
        }
    }
}
