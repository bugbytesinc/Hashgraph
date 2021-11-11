using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Proto
{
    public sealed partial class TokenAssociateTransactionBody : INetworkTransaction
    {
        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { TokenAssociate = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { TokenAssociate = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new TokenService.TokenServiceClient(channel).associateTokensAsync;
        }

        void INetworkTransaction.CheckReceipt(NetworkResult result)
        {
            if (result.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException(string.Format("Unable to associate Token with Account, status: {0}", result.Receipt.Status), result);
            }
        }

        internal TokenAssociateTransactionBody(Address token, Address account) : this()
        {
            Tokens.Add(new TokenID(token));
            Account = new AccountID(account);
        }
        internal TokenAssociateTransactionBody(IEnumerable<Address> tokens, Address account) : this()
        {
            if (tokens is null)
            {
                throw new ArgumentNullException(nameof(tokens), "The list of tokens cannot be null.");
            }
            Tokens.AddRange(tokens.Select(token =>
            {
                if (token.IsNullOrNone())
                {
                    throw new ArgumentOutOfRangeException(nameof(tokens), "The list of tokens cannot contain an empty or null address.");
                }
                return new TokenID(token);
            }));
            if (Tokens.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tokens), "The list of tokens cannot be empty.");
            }
            Account = new AccountID(account);
        }
    }
}
