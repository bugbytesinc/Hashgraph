using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Proto
{
    public sealed partial class TokenFeeScheduleUpdateTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Unable to Update Token Transfer Commissions, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            throw new InvalidOperationException("Updating Token Commissions is not a schedulable transaction.");
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { TokenFeeScheduleUpdate = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new TokenService.TokenServiceClient(channel).updateTokenFeeScheduleAsync;
        }

        internal TokenFeeScheduleUpdateTransactionBody(Address token, IEnumerable<ICommission>? commissions) : this()
        {
            TokenId = new TokenID(token);
            // Note: Null & Empty are Valid, they will clear the list of fees.
            if (commissions is not null)
            {
                CustomFees.AddRange(commissions.Select(commission => new CustomFee(commission)));
            }
        }
    }
}
