using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Proto;

public sealed partial class CryptoDeleteAllowanceTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { CryptoDeleteAllowance = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { CryptoDeleteAllowance = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(GrpcChannel channel)
    {
        return new CryptoService.CryptoServiceClient(channel).deleteAllowancesAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Unable to delete allowance, status: {0}", result.Receipt.Status), result);
        }
    }

    internal CryptoDeleteAllowanceTransactionBody(Address token, Address owner, IReadOnlyCollection<long> serialNumbers) : this()
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token), "Token is missing. Please check that it is not null.");
        }
        if (owner is null)
        {
            throw new ArgumentNullException(nameof(owner), "Owning account address is missing. Please check that it is not null.");
        }
        if (serialNumbers is null)
        {
            throw new ArgumentNullException(nameof(serialNumbers), "The list of serial numbers is missing. Please check that it is not null.");
        }
        if (serialNumbers.Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(serialNumbers), "The list of serial must contain at least one serial number to remove.");
        }
        var nftRemoveAllowance = new NftRemoveAllowance()
        {
            TokenId = new TokenID(token),
            Owner = new AccountID(owner),
        };
        nftRemoveAllowance.SerialNumbers.AddRange(serialNumbers);
        NftAllowances.Add(nftRemoveAllowance);
    }
}